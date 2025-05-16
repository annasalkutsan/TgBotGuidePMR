using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgBotGuide.Application.Interfaces;
using TgBotGuide.Bot.Interfaces;

namespace TgBotGuide.Bot.Services;

public class MenuService(
    ITelegramBotClient botClient,
    ICityService cityService,
    ILocationService locationService)
    : IMenuService
{
    private readonly Dictionary<long, int> _lastMessageIds = new();

    private async Task DeleteLastMessageAsync(long chatId)
    {
        if (_lastMessageIds.TryGetValue(chatId, out var messageId))
        {
            try
            {
                await botClient.DeleteMessage(chatId, messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting message: {ex.Message}, attempt {ex.StackTrace}, inner {ex.InnerException}");
            }
        }
    }

    public async Task ShowStartMenu(long chatId)
    {
        await DeleteLastMessageAsync(chatId);

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Выбрать город", "choose_city") },
            new[] { InlineKeyboardButton.WithCallbackData("О боте", "info") }
        });

        var sentMessage = await botClient.SendTextMessageAsync(chatId,
            "✨ Привет!\nЯ — ваш бот-гид по достопримечательностям Приднестровья. Помогу выбрать интересные места для прогулок и экскурсий! 🗺️\nВыберите пункт из меню ниже — и начнём путешествие вместе!",
            replyMarkup: inlineKeyboard);

        _lastMessageIds[chatId] = sentMessage.MessageId;
    }

    public async Task ShowBotInfo(long chatId)
    {
        await DeleteLastMessageAsync(chatId);

        var infoText =
            $"<b>Этот бот поможет вам найти интересные места в Приднестровье!</b> \n\n" +
            $"📌 С помощью меню с кнопками вы можете:\n" +
            $"— Просматривать список городов Приднестровья\n" +
            $"— Просматривать список локаций в этих городах (и их районах)\n" +
            $"— Узнавать подробную информацию о достопримечательностях\n" +
            $"⚠️ Если вы заметили неточности (неактуальная ссылка, некорректное описание и т.д.) или что-то не работает — напишите модератору бота:\n" +
            $"👤 <b>Модератор</b>: @salkutsananna\n" +
            $"📝 Также вы можете оставить предложение по улучшению бота через <b>Google-форму</b> — <a href=\"https://forms.gle/wdwUVvLEqje1dAhP8\">https://forms.gle/wdwUVvLEqje1dAhP8</a>, ваши идеи просматриваются каждую неделю\n" +
            $"❤️ Спасибо, что пользуетесь нашим ботом! <b>Желаем вам приятных путешествий по Приднестровью!</b>";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", "start_menu") }
        });

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: infoText,
            replyMarkup: inlineKeyboard,
            parseMode: ParseMode.Html
        );

        _lastMessageIds[chatId] = sentMessage.MessageId;
    }

    public async Task ShowCitySelection(long chatId)
    {
        await DeleteLastMessageAsync(chatId);

        var cities = await cityService.GetAllAsync(CancellationToken.None);
        var inlineKeyboard = new InlineKeyboardMarkup(cities.Select(city =>
            new[] { InlineKeyboardButton.WithCallbackData(city.Name, $"city_{city.Id}") }
        ).ToArray());

        inlineKeyboard.InlineKeyboard = inlineKeyboard.InlineKeyboard.Concat(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", "start_menu") }
        }).ToArray();

        var sentMessage = await botClient.SendTextMessageAsync(chatId, "Выберите город:", replyMarkup: inlineKeyboard);
        _lastMessageIds[chatId] = sentMessage.MessageId;
    }

    public async Task ShowCityDetails(Guid cityId, long chatId)
    {
        var city = await cityService.GetByIdAsync(cityId, CancellationToken.None);
        var locations = await locationService.FindAsync(location => location.CityId == cityId, CancellationToken.None);

        var inlineKeyboard = new InlineKeyboardMarkup(locations.Select(location =>
            new[] { InlineKeyboardButton.WithCallbackData(location.Name, $"location_{location.Name}") }
        ).ToArray());

        inlineKeyboard.InlineKeyboard = inlineKeyboard.InlineKeyboard.Concat(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", "choose_city") }
        }).ToArray();

        var sentMessage = await botClient.SendTextMessageAsync(chatId,
            $"Вы выбрали город {city.Name}.\n" +
            $"Описание: {city.Description}\n" +
            $"Вот места, которые мы советуем вам посетить:",
            replyMarkup: inlineKeyboard);

        _lastMessageIds[chatId] = sentMessage.MessageId;
    }

    public async Task ShowLocationDetails(Guid locationId, long chatId)
    {
        await DeleteLastMessageAsync(chatId);
        var location = await locationService.GetByIdAsync(locationId, CancellationToken.None);

        var locationDetails = $"Название локации: {location.Name}\n" +
                              $"Описание: {location.Description}\n" +
                              $"Ссылка на Google Maps: {location.MapUrl}\n";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", $"city_{location.CityId}") }
        });

        var sentMessage = await botClient.SendPhotoAsync(
            chatId,
            location.ImageUrl,
            caption: locationDetails,
            replyMarkup: inlineKeyboard
        );

        _lastMessageIds[chatId] = sentMessage.MessageId;
    }

    public async Task OnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        var data = callbackQuery.Data;
        var chatId = callbackQuery.Message.Chat.Id;

        if (data.StartsWith("location_"))
        {
            var locationName = data.Substring(9);
            var locations =
                await locationService.FindAsync(location => location.Name == locationName, CancellationToken.None);

            if (locations.Any())
            {
                var location = locations.First();
                await ShowLocationDetails(location.Id, chatId);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Локация не найдена.");
            }
        }
        else if (data == "choose_city")
        {
            await ShowCitySelection(chatId);
        }
        else if (data.StartsWith("city_"))
        {
            var cityId = Guid.Parse(data.Substring(5));
            await ShowCityDetails(cityId, chatId);
        }
        else if (data.StartsWith("info"))
        {
            await ShowBotInfo(chatId);
        }
        else if (data == "start_menu")
        {
            await ShowStartMenu(chatId);
        }
    }
}
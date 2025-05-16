using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBotGuide.Bot.Interfaces;

namespace TgBotGuide.Bot.Services;

public class TelegramBotService(ITelegramBotClient botClient, IMenuService menuService)
{
    public async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions();

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;

            if (string.IsNullOrEmpty(update.Message.Text))
            {
                await SendStartButton(chatId);
            }
            else if (update.Message.Text == "/start")
            {
                await menuService.ShowStartMenu(chatId);
            }
            else if (update.Message.Text == "/info")
            {
                await menuService.ShowBotInfo(chatId);
            }
            else if (update.Message.Text == "/moderator")
            {
                await botClient.SendTextMessageAsync(chatId,
                    "Связаться с модератором вы можете по нику: @salkutsananna",
                    cancellationToken: cancellationToken);
            }
            else if (update.Message.Text == "/agencies")
            {
                await botClient.SendTextMessageAsync(chatId,
                    "🌍 Туристические агентства в ПМР:\n" +
                    "1. Хочу туда — @hoshutuda",
                    cancellationToken: cancellationToken);
            }
            else if (update.Message.Text == "/suggestions")
            {
                await botClient.SendTextMessageAsync(chatId,
                    "Если у вас есть предложения по улучшению бота — отправьте их сюда <a href=\"https://forms.gle/wdwUVvLEqje1dAhP8\">https://forms.gle/wdwUVvLEqje1dAhP8</a>",
                    cancellationToken: cancellationToken);
            }
        }
        else if (update.CallbackQuery != null)
        {
            await menuService.OnCallbackQueryReceived(update.CallbackQuery);
        }
    }


    private async Task SendStartButton(long chatId)
    {
        var startKeyboard = new InlineKeyboardMarkup(
            new[] { InlineKeyboardButton.WithCallbackData("🚀 Начать", "start_menu") }
        );

        await botClient.SendTextMessageAsync(chatId,
            "Добро пожаловать! Нажмите кнопку ниже, чтобы начать:",
            replyMarkup: startKeyboard);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
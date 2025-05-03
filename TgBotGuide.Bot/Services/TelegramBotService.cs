using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TgBotGuide.Bot.Interfaces;

namespace TgBotGuide.Bot.Services;

public class TelegramBotService(ITelegramBotClient botClient, IMenuService menuService)
{
    public async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { },
        };

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
            if (update.Message.Text == "/start")
            {
                await menuService.ShowStartMenu(chatId);
            }
        }
        else if (update.CallbackQuery != null)
        {
            await menuService.OnCallbackQueryReceived(update.CallbackQuery);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
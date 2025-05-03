﻿using Telegram.Bot.Types;

namespace TgBotGuide.Bot.Interfaces;

public interface IMenuService
{
    Task ShowStartMenu(long chatId);
    Task ShowBotInfo(long chatId);
    Task ShowCitySelection(long chatId);
    Task ShowCityDetails(Guid cityId, long chatId);
    Task ShowLocationDetails(Guid locationId, long chatId);
    Task OnCallbackQueryReceived(CallbackQuery callbackQuery);
}
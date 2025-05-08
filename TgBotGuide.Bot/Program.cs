using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TgBotGuide.Application;
using TgBotGuide.Application.Interfaces;
using TgBotGuide.Application.Mapping;
using TgBotGuide.Application.Services;
using TgBotGuide.Bot.Interfaces;
using TgBotGuide.Bot.Services;
using TgBotGuide.Domain.Interfaces;
using TgBotGuide.Infrastructure.Bot.Options;
using TgBotGuide.Infrastructure.DataBase;
using TgBotGuide.Infrastructure.DataBase.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DataBase");
builder.Services.AddDbContext<TgBotGuideDbContext>(options => { options.UseNpgsql(connectionString); });

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityService, CityService>();

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.Configure<TelegramBotOptions>(builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    return new TelegramBotClient(options.Token);
});
builder.Services.AddScoped<TelegramBotService>(provider =>
{
    var botClient = provider.GetRequiredService<ITelegramBotClient>();
    return new TelegramBotService(botClient, provider.GetRequiredService<IMenuService>());
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    await botClient.DeleteWebhookAsync(); // <-- удаляем webhook

    var telegramBotService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
    var cancellationToken = app.Lifetime.ApplicationStopping;
    await telegramBotService.StartPollingAsync(cancellationToken);
}

app.MapGet("/", () => "Hello World!");

app.Run();
using Microsoft.Extensions.DependencyInjection;
using ReviewGrabberBot.Handlers;
using ReviewGrabberBot.Services;
using Telegram.Bot;

namespace ReviewGrabberBot.Extensions
{
    internal static class ServicesExtensions
    {
        internal static void ConfigureServices(this IServiceCollection services, string botToken)
        {
            services.AddSingleton<Context>();
            services.AddSingleton(new TelegramBotClient(botToken));
            services.AddSingleton<UpdateHandler>();

            services.AddHostedService<BotHandlerService>();
            services.AddHostedService<ScriptRunnerService>();
            services.AddHostedService<BotNotifierService>();
        }
    }
}
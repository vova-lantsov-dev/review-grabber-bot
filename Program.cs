using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReviewGrabberBot.Extensions;
using ReviewGrabberBot.Handlers;
using ReviewGrabberBot.Services;
using Telegram.Bot;

namespace ReviewGrabberBot
{
    internal sealed class Program
    {
        internal static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddEnvironmentVariables("REVIEWBOT_");
                })
                .ConfigureServices((context, services) =>
                {
                    services.ConfigureOptions(context.Configuration, out var botToken);

                    services.AddSingleton<Context>();
                    services.AddSingleton(new TelegramBotClient(botToken));
                    services.AddSingleton<UpdateHandler>();

                    services.AddHostedService<BotHandlerService>();
                    services.AddHostedService<ScriptRunnerService>();
                    services.AddHostedService<BotNotifierService>();
                })
                .RunConsoleAsync();
        }
    }
}
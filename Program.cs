using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReviewGrabberBot.Options;
using ReviewGrabberBot.Services;
using Telegram.Bot;

namespace ReviewGrabberBot
{
    public class Program
    {
        internal static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<ContextOptions>(options =>
                    {
                        options.ConnectionString = "mongodb://localhost";
                        options.DatabaseName = "reviewbot";
                        options.ReviewCollectionName = "reviews";
                    });
                    services.Configure<BotOptions>(options =>
                    {
                        options.AdminId = "336510341";
                    });

                    services.AddSingleton<Context>();
                    services.AddSingleton(new TelegramBotClient(Constants.BotToken));
                })
                .RunConsoleAsync();
        }
    }
}
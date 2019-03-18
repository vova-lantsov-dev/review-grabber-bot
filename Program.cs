using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReviewGrabberBot.Extensions;

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
                    services.ConfigureServices(botToken);
                })
                .RunConsoleAsync();
        }
    }
}
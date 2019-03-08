using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ReviewGrabberBot.Exceptions;
using ReviewGrabberBot.Handlers;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;
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
                    var config = context.Configuration;
                    services.Configure<ContextOptions>(options =>
                    {
                        options.ConnectionString = config["CONNECTION_STRING"] ?? "mongodb://localhost";
                        options.DatabaseName = "reviewbot";
                        options.ReviewCollectionName = "reviews";
                    });
                    services.Configure<BotOptions>(options =>
                    {
                        options.AdminId = config["ADMIN_ID"]
                            ?? throw new EnvironmentVariableNotFoundException("ADMIN_ID");
                    });
                    services.Configure<NotifierOptions>(options =>
                    {
                        options.WorkingDirectory = config["WORKING_DIRECTORY"]
                            ?? throw new EnvironmentVariableNotFoundException("WORKING_DIRECTORY");
                        if (!Directory.Exists(options.WorkingDirectory))
                            throw new EnvironmentVariableDirectoryNotFoundException("WORKING_DIRECTORY");

                        options.ScrapyPath = config["SCRAPY_PATH"]
                            ?? throw new EnvironmentVariableNotFoundException("SCRAPY_PATH");
                        if (!File.Exists(options.ScrapyPath))
                            throw new EnvironmentVariableFileNotFoundException("SCRAPY_PATH");
                        
                        var notifierOptionsPath = config["NOTIFIER_OPTIONS_PATH"]
                            ?? throw new EnvironmentVariableNotFoundException("NOTIFIER_OPTIONS_PATH");
                        if (!File.Exists(notifierOptionsPath))
                            throw new EnvironmentVariableFileNotFoundException("NOTIFIER_OPTIONS_PATH");
                        
                        var json = File.ReadAllText(notifierOptionsPath);
                        options.Restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(json);
                    });

                    services.AddSingleton<Context>();
                    services.AddSingleton(new TelegramBotClient(Constants.BotToken));
                    services.AddSingleton<UpdateHandler>();

                    services.AddHostedService<BotHandlerService>();
                    services.AddHostedService<ScriptRunnerService>();
                    services.AddHostedService<BotNotifierService>();
                })
                .RunConsoleAsync();
        }
    }
}
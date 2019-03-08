using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ReviewGrabberBot.Handlers;
using ReviewGrabberBot.Models;
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
                            ?? throw new Exception("REVIEWBOT_ADMIN_ID environment variable was not found");
                    });
                    services.Configure<NotifierOptions>(options =>
                    {
                        options.WorkingDirectory = config["WORKING_DIRECTORY"]
                           ?? throw new Exception(
                               "REVIEWBOT_WORKING_DIRECTORY environment variable was not found");
                        
                        if (!Directory.Exists(options.WorkingDirectory))
                            throw new DirectoryNotFoundException("REVIEWBOT_WORKING_DIRECTORY directory was not found");
                        
                        var notifierOptionsPath = config["NOTIFIER_OPTIONS_PATH"]
                            ?? throw new Exception(
                                "REVIEWBOT_NOTIFIER_OPTIONS_PATH environment variable was not found");
                        
                        if (!File.Exists(notifierOptionsPath))
                            throw new FileNotFoundException("REVIEW_NOTIFIER_OPTIONS_PATH file was not found");
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
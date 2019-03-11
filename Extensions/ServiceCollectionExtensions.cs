using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReviewGrabberBot.Exceptions;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static void ConfigureOptions(this IServiceCollection services, IConfiguration config, out string botToken)
        {
            services.Configure<ContextOptions>(options =>
            {
                options.ConnectionString = config["CONNECTION_STRING"] ?? "mongodb://localhost";
                options.DatabaseName = "reviewbot";
                options.ReviewCollectionName = "reviews";
            });
            services.Configure<BotOptions>(options =>
            {
                if (!int.TryParse(config["ADMIN_ID"]
                    ?? throw new EnvironmentVariableNotFoundException("ADMIN_ID"),
                    out options.AdminId))
                    throw new EnvironmentVariableMustBeIntegerException("ADMIN_ID");
            });
            services.Configure<NotifierOptions>(options =>
            {
                options.WorkingDirectory = config["WORKING_DIRECTORY"]
                    ?? throw new EnvironmentVariableNotFoundException("WORKING_DIRECTORY");
                if (!Directory.Exists(options.WorkingDirectory))
                    throw new EnvironmentVariableDirectoryNotFoundException("WORKING_DIRECTORY");

                options.FileName = config["FILE_NAME"]
                    ?? throw new EnvironmentVariableNotFoundException("FILE_NAME");

                options.Arguments = config["ARGUMENTS"]
                    ?? throw new EnvironmentVariableNotFoundException("ARGUMENTS");
                        
                var notifierOptionsPath = config["NOTIFIER_OPTIONS_PATH"]
                    ?? throw new EnvironmentVariableNotFoundException("NOTIFIER_OPTIONS_PATH");
                if (!File.Exists(notifierOptionsPath))
                    throw new EnvironmentVariableFileNotFoundException("NOTIFIER_OPTIONS_PATH");
                        
                var json = File.ReadAllText(notifierOptionsPath);
                options.Restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(json);
            });

            botToken = config["BOT_TOKEN"]
                ?? throw new EnvironmentVariableNotFoundException("BOT_TOKEN");
        }
    }
}
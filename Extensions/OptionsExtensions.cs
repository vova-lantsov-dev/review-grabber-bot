using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReviewGrabberBot.Exceptions;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Extensions
{
    internal static class OptionsExtensions
    {
        internal static void ConfigureOptions(this IServiceCollection services, IConfiguration config, out string botToken)
        {
            services.Configure<ContextOptions>(options =>
            {
                options.ConnectionString = config["CONNECTION_STRING"];
                options.DatabaseName = "reviewbot";
                options.ReviewCollectionName = "reviews";
            });
            // ReSharper disable once RedundantAssignment
            services.Configure<ReviewGrabberOptions>(options =>
            {
                var reviewGrabberOptionsPath = config["REVIEW_GRABBER_OPTIONS_PATH"]
                    ?? throw new EnvironmentVariableNotFoundException("REVIEW_GRABBER_OPTIONS_PATH");

                var json = File.ReadAllText(reviewGrabberOptionsPath);
                options = JsonConvert.DeserializeObject<ReviewGrabberOptions>(json);
            });

            botToken = config["BOT_TOKEN"]
                ?? throw new EnvironmentVariableNotFoundException("BOT_TOKEN");
        }
    }
}
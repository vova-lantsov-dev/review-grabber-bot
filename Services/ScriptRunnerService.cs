using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Services
{
    internal sealed class ScriptRunnerService : BackgroundService
    {
        private readonly List<Restaurant> _restaurants;
        private readonly string _workingDirectory;
        private readonly string _fileName;
        private readonly string _arguments;
        
        public ScriptRunnerService(IOptions<NotifierOptions> options)
        {
            _restaurants = options.Value.Restaurants;
            _workingDirectory = options.Value.WorkingDirectory;
            _fileName = options.Value.FileName;
            _arguments = options.Value.Arguments;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.WhenAll(GetNotifierTask(stoppingToken),
                        Task.Delay(TimeSpan.FromMinutes(60d), stoppingToken));
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
            }
        }

        private Task GetNotifierTask(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var reviewPlatforms = new Dictionary<string, Func<Restaurant, string>>
                {
                    ["tripadvisor"] = r => r.TripadvisorUrl,
                    ["google"] = r => r.GoogleUrl,
                    ["foursquare"] = r => r.FoursquareUrl,
                    ["restoclub"] = r => r.RestoclubUrl,
                    ["restorating"] = r => r.RestoratingUrl,
                    ["yell"] = r => r.YellUrl,
                    ["facebook"] = r => r.FacebookUrl,
                    ["yandex"] = r => r.YandexUrl,
                    ["vk"] = r => r.VkUrl
                };

                foreach (var restaurant in _restaurants)
                foreach (var (resource, link) in reviewPlatforms)
                {
                    var processInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = _workingDirectory,
                        Arguments = string.Format(_arguments, resource, link(restaurant), restaurant.Name),
                        FileName = _fileName
                    };
                    var process = Process.Start(processInfo);
                    process?.WaitForExit();
                }
            }, cancellationToken);
        }
    }
}
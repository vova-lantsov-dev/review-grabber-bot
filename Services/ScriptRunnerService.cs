using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Services
{
    internal sealed class ScriptRunnerService : BackgroundService
    {
        private readonly ILogger<ScriptRunnerService> _logger;
        private readonly List<Restaurant> _restaurants;
        private readonly ScriptRunnerData _scriptRunnerData;
        
        public ScriptRunnerService(IOptions<ReviewGrabberOptions> options, ILogger<ScriptRunnerService> logger)
        {
            _restaurants = options.Value.NotifierData.Restaurants;
            _scriptRunnerData = options.Value.ScriptRunnerData;
            _logger = logger;
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
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occurred while running a WhenAll method");
                }
            }
        }

        private Task GetNotifierTask(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                foreach (var restaurant in _restaurants)
                foreach (var (resource, link) in restaurant.Urls)
                {
                    var processInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = _scriptRunnerData.WorkingDirectory,
                        Arguments = string.Format(_scriptRunnerData.Arguments, resource, link, restaurant.Name),
                        FileName = _scriptRunnerData.FileName
                    };
                    var process = Process.Start(processInfo);
                    process?.WaitForExit();
                }
            }, cancellationToken);
        }
    }
}
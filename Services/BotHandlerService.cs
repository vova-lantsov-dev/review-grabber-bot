using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ReviewGrabberBot.Handlers;
using Telegram.Bot;

namespace ReviewGrabberBot.Services
{
    internal sealed class BotHandlerService : BackgroundService
    {
        private readonly UpdateHandler _updateHandler;
        private readonly TelegramBotClient _client;
        
        internal BotHandlerService(UpdateHandler updateHandler, TelegramBotClient client)
        {
            _updateHandler = updateHandler;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.ReceiveAsync(_updateHandler, stoppingToken);
        }
    }
}
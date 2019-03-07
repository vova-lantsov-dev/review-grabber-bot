using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReviewGrabberBot.Services
{
    public sealed class BotNotifierService : BackgroundService
    {
        private readonly TelegramBotClient _client;
        private readonly Context _context;
        private readonly int _adminId;
        
        public BotNotifierService(Context context, TelegramBotClient client, IOptions<BotOptions> options)
        {
            if (!int.TryParse(options.Value.AdminId, out _adminId))
                throw new ArgumentException("Admin id must be integer");
            
            _context = context;
            _client = client;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1d), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notSentReviews = await _context.Reviews.Find(r => r.NeedToShow).ToListAsync(stoppingToken);
                    foreach (var notSentReview in notSentReviews)
                    {
                        var buttons = new List<List<InlineKeyboardButton>>();
                        if ((notSentReview.Comments?.Count ?? 0) > 0)
                            buttons.Add(new List<InlineKeyboardButton>
                            {
                                new InlineKeyboardButton { Text = "Просмотреть отзывы",
                                    CallbackData = $"comments~{notSentReview.Id}" }
                            });
                        if (!notSentReview.IsReadOnly)
                            buttons.Add(new List<InlineKeyboardButton>
                            {
                                new InlineKeyboardButton { Text = "Открыть отзыв", Url = notSentReview.ReplyLink }
                            });
                        await _client.SendTextMessageAsync(_adminId, notSentReview.ToString(), ParseMode.Markdown,
                            cancellationToken: stoppingToken, replyMarkup: buttons.Count > 0
                                ? new InlineKeyboardMarkup(buttons)
                                : null);
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
            }
        }
    }
}
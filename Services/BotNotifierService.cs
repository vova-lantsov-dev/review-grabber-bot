using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReviewGrabberBot.Services
{
    internal sealed class BotNotifierService : BackgroundService
    {
        private readonly TelegramBotClient _client;
        private readonly Context _context;
        private readonly int _adminId;
        
        public BotNotifierService(Context context, TelegramBotClient client, IOptions<BotOptions> options)
        {
            _adminId = options.Value.AdminId;
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
                    await Task.WhenAll(GetNotifierTask(stoppingToken),
                        Task.Delay(TimeSpan.FromMinutes(60d), stoppingToken));
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task GetNotifierTask(CancellationToken cancellationToken)
        {
            var notSentReviews = await _context.Reviews.Find(r => r.NeedToShow).ToListAsync(cancellationToken);
            foreach (var notSentReview in notSentReviews)
            {
                var buttons = new List<List<InlineKeyboardButton>>();
                if ((notSentReview.Comments?.Count ?? 0) > 0)
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        new InlineKeyboardButton { Text = "Просмотреть отзывы",
                            CallbackData = $"comments~{notSentReview.Id}" }
                    });
                if (!notSentReview.IsReadOnly && notSentReview.ReplyLink != null)
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        new InlineKeyboardButton { Text = "Открыть отзыв", Url = notSentReview.ReplyLink }
                    });
                await _client.SendTextMessageAsync(_adminId, notSentReview.ToString(), ParseMode.Markdown,
                    cancellationToken: cancellationToken, replyMarkup: buttons.Count > 0
                        ? new InlineKeyboardMarkup(buttons)
                        : null);
                await _context.Reviews.UpdateOneAsync(r => r.Id == notSentReview.Id,
                    Builders<Review>.Update.Set(r => r.NeedToShow, false),
                    cancellationToken: cancellationToken);
            }
        }
    }
}
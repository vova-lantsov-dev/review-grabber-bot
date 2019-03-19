using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<BotNotifierService> _logger;
        private readonly TelegramBotClient _client;
        private readonly Context _context;
        private readonly long _adminId;
        private readonly Dictionary<string, int> _maxValuesOfRating;
        
        public BotNotifierService(Context context, TelegramBotClient client, IOptions<BotOptions> options,
            IOptions<NotifierOptions> notifierOptions, ILogger<BotNotifierService> logger)
        {
            _logger = logger;
            _adminId = options.Value.AdminId;
            _maxValuesOfRating = notifierOptions.Value.Data.MaxValuesOfRating;
            _context = context;
            _client = client;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(5d), stoppingToken);

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
                if (!notSentReview.IsReadOnly && notSentReview.ReplyLink != null && notSentReview.Resource != "google")
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        new InlineKeyboardButton { Text = "Открыть отзыв", Url = notSentReview.ReplyLink }
                    });

                var sentMessage = await _client.SendTextMessageAsync(_adminId, notSentReview.ToString(
                        _maxValuesOfRating.TryGetValue(notSentReview.Resource, out var maxValueOfRating)
                            ? maxValueOfRating : -1), ParseMode.Markdown,
                    cancellationToken: cancellationToken, replyMarkup: buttons.Count > 0
                        ? new InlineKeyboardMarkup(buttons) : null);

                if (notSentReview.Resource == "google")
                {
                    await _context.GoogleReviewMessages.UpdateOneAsync(grm => grm.ReviewId == notSentReview.Id,
                        Builders<GoogleReviewMessage>.Update.Set(grm => grm.MessageId, sentMessage.MessageId),
                        new UpdateOptions {IsUpsert = true}, cancellationToken);
                }

                // ReSharper disable once MethodSupportsCancellation
                await _context.Reviews.UpdateOneAsync(r => r.Id == notSentReview.Id,
                    Builders<Review>.Update.Set(r => r.NeedToShow, false));
                
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}
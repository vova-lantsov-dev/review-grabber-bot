using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal sealed class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly ScriptRunnerData _scriptRunnerData;
        private readonly NotifierData _notifierData;
        private readonly Context _context;
        private readonly TelegramBotClient _client;
        
        public WorkerService(IOptions<ReviewGrabberOptions> options, ILogger<WorkerService> logger,
            Context context, TelegramBotClient client)
        {
            _notifierData = options.Value.NotifierData;
            _scriptRunnerData = options.Value.ScriptRunnerData;
            _logger = logger;
            _context = context;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.WhenAll(GetWorkerTask(stoppingToken),
                        Task.Delay(TimeSpan.FromMinutes(60d), stoppingToken));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occurred while running WhenAll method");
                }
            }
        }

        private async Task GetWorkerTask(CancellationToken cancellationToken)
        {
            try
            {
                var initialCount = await _context.Reviews.CountDocumentsAsync(FilterDefinition<Review>.Empty,
                    cancellationToken: cancellationToken);
                
                await GetScriptRunnerTask(cancellationToken);
                await GetNotifierTask(initialCount, cancellationToken);
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                _logger.LogError(e, "Error occurred while running worker task");
            }
        }

        private Task GetScriptRunnerTask(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                foreach (var restaurant in _notifierData.Restaurants)
                foreach (var (resource, link) in restaurant.Urls)
                {
                    cancellationToken.ThrowIfCancellationRequested();
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
        
        private async Task GetNotifierTask(long initialCountOfReviews, CancellationToken cancellationToken)
        {
            if (initialCountOfReviews == 0)
            {
                // ReSharper disable once MethodSupportsCancellation
                await _context.Reviews.UpdateManyAsync(FilterDefinition<Review>.Empty,
                    Builders<Review>.Update.Set(r => r.NeedToShow, false));
                return;
            }
            
            var notSentReviews = await _context.Reviews.Find(r => r.NeedToShow).ToListAsync(cancellationToken);
            foreach (var notSentReview in notSentReviews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var buttons = new List<List<InlineKeyboardButton>>();
                if ((notSentReview.Comments?.Count ?? 0) > 0)
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        new InlineKeyboardButton
                        {
                            Text = "Просмотреть отзывы",
                            CallbackData = $"comments~{notSentReview.Id}"
                        }
                    });
                if (!notSentReview.IsReadOnly && notSentReview.ReplyLink != null &&
                    notSentReview.Resource != "google")
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        new InlineKeyboardButton {Text = "Открыть отзыв", Url = notSentReview.ReplyLink}
                    });

                var chatId = _notifierData.Restaurants.Find(r => r.Name == notSentReview.RestaurantName).ChatId;
                var sentMessage = await _client.SendTextMessageAsync(chatId, notSentReview.ToString(
                        _notifierData.MaxValuesOfRating.TryGetValue(notSentReview.Resource,
                            out var maxValueOfRating)
                            ? maxValueOfRating
                            : -1,
                        _notifierData.PreferAvatarOverProfileLinkFor.Contains(notSentReview.Resource)),
                    ParseMode.Markdown, cancellationToken: cancellationToken, replyMarkup: buttons.Count > 0
                        ? new InlineKeyboardMarkup(buttons)
                        : null);

                if (notSentReview.Resource == "google")
                    await _context.GoogleReviewMessages.UpdateOneAsync(grm => grm.ReviewId == notSentReview.Id,
                        Builders<GoogleReviewMessage>.Update.Set(grm => grm.MessageId, sentMessage.MessageId),
                        new UpdateOptions {IsUpsert = true}, cancellationToken);

                // ReSharper disable once MethodSupportsCancellation
                await _context.Reviews.UpdateOneAsync(r => r.Id == notSentReview.Id,
                    Builders<Review>.Update.Set(r => r.NeedToShow, false));
            }
        }
    }
}
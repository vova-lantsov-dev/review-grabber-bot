using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Services;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReviewGrabberBot.Handlers
{
    internal sealed class UpdateHandler : IUpdateHandler
    {
        private readonly TelegramBotClient _client;
        private readonly BotData _botData;
        private readonly Context _context;
        private readonly ILogger<UpdateHandler> _logger;
        private readonly HttpClient _httpClient;
        
        public UpdateHandler(TelegramBotClient client, Context context, IOptions<Options.ReviewGrabberOptions> options,
            ILogger<UpdateHandler> logger, HttpClient httpClient)
        {
            _botData = options.Value.BotData;
            _client = client;
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }
        
        public async Task HandleUpdate(Update update, CancellationToken cancellationToken)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (update.Type)
            {
                case UpdateType.CallbackQuery when update.CallbackQuery.Message != null &&
                                                   update.CallbackQuery.Message.Chat.Id == _botData.ChatId &&
                                                   _botData.AdminIds.Contains(update.CallbackQuery.From.Id):
                {
                    var q = update.CallbackQuery;
                    var separated = q.Data.Split('~');
                    if (separated.Length == 0)
                        return;

                    switch (separated[0])
                    {
                        case "comments" when separated.Length == 2:
                        {
                            var review = await _context.Reviews.Find(r => r.Id == separated[1])
                                .SingleOrDefaultAsync(cancellationToken);
                            if (review == default)
                                break;

                            await _client.EditMessageTextAsync(_botData.ChatId, q.Message.MessageId,
                                string.Concat(review, "\n\n", "*Комментарии:*", "\n\n",
                                    string.Join("\n\n", review.Comments)),
                                ParseMode.Markdown, replyMarkup: !review.IsReadOnly && review.ReplyLink != null
                                    ? new InlineKeyboardButton {Text = "Открыть отзыв", Url = review.ReplyLink}
                                    : null, cancellationToken: cancellationToken);

                            break;
                        }

                        default:
                        {
                            await _client.SendTextMessageAsync(_botData.ChatId,
                                string.Concat($"*Received bad request*\n\n```separated[1] == \"{separated[1]}\"```\n\n",
                                    "Maybe, something works wrong. Please, contact the developer."),
                                ParseMode.Markdown, cancellationToken: cancellationToken);

                            break;
                        }
                    }

                    await _client.AnswerCallbackQueryAsync(update.CallbackQuery.Id,
                        cancellationToken: cancellationToken);
                    
                    break;
                }

                case UpdateType.Message
                    when update.Message.Type == MessageType.Text && update.Message.ReplyToMessage != null:
                {
                    var m = update.Message;

                    var googleReviewMessage = await _context.GoogleReviewMessages
                        .Find(grm => grm.MessageId == m.ReplyToMessage.MessageId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (googleReviewMessage == default)
                        return;

                    var review = await _context.Reviews.Find(r => r.Id == googleReviewMessage.ReviewId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (review == default)
                    {
                        await _context.GoogleReviewMessages.DeleteOneAsync(r =>
                            r.ReviewId == googleReviewMessage.ReviewId, cancellationToken);
                        return;
                    }

                    var googleCredential = await _context.GoogleCredentials.Find(gc => gc.Name == "google")
                        .FirstOrDefaultAsync(cancellationToken);
                    if (googleCredential == default)
                        return;

                    var serializer = new DataContractJsonSerializer(typeof(GoogleCommentModel));
                    string jsonContent;
                    using (var jsonStream = new MemoryStream())
                    {
                        serializer.WriteObject(jsonStream, new GoogleCommentModel
                        {
                            Comment = update.Message.Text
                        });
                        jsonContent = Encoding.UTF8.GetString(jsonStream.ToArray());
                    }

                    var httpRequest = new HttpRequestMessage(HttpMethod.Put, review.ReplyLink);
                    httpRequest.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", googleCredential.AccessToken);
                    httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    await _httpClient.SendAsync(httpRequest, cancellationToken);
                    
                    await _client.SendTextMessageAsync(m.Chat, "Ответ на отзыв успешно отправлен!",
                        replyToMessageId: m.MessageId, cancellationToken: cancellationToken);
                    
                    break;
                }
            }
        }

        public async Task HandleError(Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                await _client.SendTextMessageAsync(_botData.ChatId,
                    $"*Error occurred while getting updates*\n\n```{exception}```\n\nPlease, contact the developer.",
                    ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while sending the request to a Telegram server");
            }
        }

        public UpdateType[] AllowedUpdates => new[] {UpdateType.CallbackQuery, UpdateType.Message};
    }
}
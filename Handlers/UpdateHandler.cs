using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Options;
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
        private readonly long _adminId;
        private readonly Context _context;
        private readonly ILogger<UpdateHandler> _logger;
        
        public UpdateHandler(TelegramBotClient client, Context context, IOptions<BotOptions> options, ILogger<UpdateHandler> logger)
        {
            _adminId = options.Value.AdminId;
            _client = client;
            _context = context;
            _logger = logger;
        }
        
        public async Task HandleUpdate(Update update, CancellationToken cancellationToken)
        {
            var q = update.CallbackQuery;
            
            if (q == null || q.InlineMessageId != null || q.Message.Chat.Type != ChatType.Private ||
                q.Message.Chat.Id != _adminId)
                return;

            var separated = q.Data.Split('~');
            if (separated.Length == 0)
                return;

            switch (separated[0])
            {
                case "comments" when separated.Length == 2:
                    var review = await _context.Reviews.Find(r => r.Id == separated[1]).SingleOrDefaultAsync(cancellationToken);
                    if (review == default)
                        break;

                    await _client.EditMessageTextAsync(_adminId, q.Message.MessageId,
                        string.Concat(review, "\n\n", "*Комментарии:*", "\n\n",
                            string.Join("\n\n", review.Comments)),
                        ParseMode.Markdown, replyMarkup: !review.IsReadOnly && review.ReplyLink != null
                            ? new InlineKeyboardButton {Text = "Открыть отзыв", Url = review.ReplyLink}
                            : null, cancellationToken: cancellationToken);

                    break;

                default:
                    await _client.SendTextMessageAsync(_adminId,
                        string.Concat($"*Received bad request*\n\n```separated[1] == \"{separated[1]}\"```\n\n",
                            "Maybe, something works wrong. Please, contact the developer."), 
                        ParseMode.Markdown, cancellationToken: cancellationToken);

                    break;
            }

            await _client.AnswerCallbackQueryAsync(q.Id, cancellationToken: cancellationToken);
        }

        public async Task HandleError(Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                await _client.SendTextMessageAsync(_adminId,
                    $"*Error occurred while getting updates*\n\n```{exception}```\n\nPlease, contact the developer.",
                    ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while while sending the request to a Telegram server");
            }
        }

        public UpdateType[] AllowedUpdates => new[] {UpdateType.CallbackQuery};
    }
}
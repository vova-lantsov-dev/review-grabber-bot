using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
        private readonly int _adminId;
        private readonly Context _context;
        private readonly CancellationToken _cancellationToken;
        
        public UpdateHandler(TelegramBotClient client, Context context, IOptions<BotOptions> options, IApplicationLifetime lifetime)
        {
            _adminId = options.Value.AdminId;
            _client = client;
            _context = context;
            _cancellationToken = lifetime.ApplicationStopping;
        }
        
        public async Task HandleUpdate(Update update)
        {
            var q = update.CallbackQuery;
            
            if (q == null || q.InlineMessageId != null || q.Message.Chat.Type != ChatType.Private ||
                q.From.Id != _adminId)
                return;

            var separated = q.Data.Split('~');
            if (separated.Length == 0)
                return;

            switch (separated[0])
            {
                case "comments" when separated.Length == 2:
                    var review = await _context.Reviews.Find(r => r.Id == separated[1]).SingleOrDefaultAsync(_cancellationToken);
                    if (review == default)
                        break;

                    try
                    {
                        await _client.EditMessageTextAsync(_adminId, q.Message.MessageId,
                            string.Concat(review, "\n\n", "*Комментарии:*", "\n\n",
                                string.Join("\n\n", review.Comments)),
                            ParseMode.Markdown, replyMarkup: !review.IsReadOnly && review.ReplyLink != null
                                ? new InlineKeyboardButton {Text = "Открыть отзыв", Url = review.ReplyLink}
                                : null, cancellationToken: _cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    break;

                default:
                    try
                    {
                        await _client.SendTextMessageAsync(_adminId,
                            string.Concat($"*Received bad request*\n\n```separated[1] == \"{separated[1]}\"```\n\n",
                                "Maybe, something works wrong. Please, contact the developer."), 
                            ParseMode.Markdown, cancellationToken: _cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    break;
            }

            try
            {
                await _client.AnswerCallbackQueryAsync(q.Id, cancellationToken: _cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task HandleError(Exception exception)
        {
            try
            {
                await _client.SendTextMessageAsync(_adminId,
                    $"*Error occurred while getting updates*\n\n```{exception}```\n\nPlease, contact the developer.",
                    ParseMode.Markdown, cancellationToken: _cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public UpdateType[] AllowedUpdates => new[] {UpdateType.CallbackQuery};
    }
}
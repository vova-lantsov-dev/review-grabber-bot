using System;
using System.Threading.Tasks;
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
    public sealed class UpdateHandler : IUpdateHandler
    {
        private readonly TelegramBotClient _client;
        private readonly int _adminId;
        private readonly Context _context;
        
        public UpdateHandler(TelegramBotClient client, Context context, IOptions<BotOptions> options)
        {
            if (!int.TryParse(options.Value.AdminId, out _adminId))
                throw new ArgumentException("Admin id must be an integer. Please, update settings file.");

            _client = client;
            _context = context;
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
                    var review = await _context.Reviews.Find(r => r.Id == separated[1]).SingleOrDefaultAsync();
                    if (review == default)
                        break;

                    try
                    {
                        await _client.EditMessageTextAsync(_adminId, q.Message.MessageId,
                            string.Concat(review, "\n\n", "*Комментарии:*", "\n\n",
                                string.Join("\n\n", review.Comments)),
                            ParseMode.Markdown, replyMarkup:
                            new InlineKeyboardButton {Text = "Открыть отзыв", Url = review.ReplyLink});
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
                                "Maybe, something works wrong. Please, contact the developer."));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    break;
            }

            try
            {
                await _client.AnswerCallbackQueryAsync(q.Id);
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
                    ParseMode.Markdown);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public UpdateType[] AllowedUpdates => new[] {UpdateType.CallbackQuery};
    }
}
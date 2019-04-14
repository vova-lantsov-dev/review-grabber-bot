using MongoDB.Bson.Serialization.Attributes;

namespace ReviewGrabberBot.Models
{
    internal sealed class GoogleReviewMessage
    {
        [BsonId] public string ReviewId;

        public int MessageId;

        public long ChatId;
    }
}
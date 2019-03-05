using MongoDB.Bson.Serialization.Attributes;

namespace ReviewGrabberBot.Models
{
    public sealed class Review
    {
        public string Id;
        
        [BsonElement("resource")] public string Resource;
        
        [BsonElement("restaurant_name")] public string RestaurantName;

        [BsonElement("need_to_show")] public bool NeedToShow;

        [BsonElement("reply_link")] public string ReplyLink;

        [BsonElement("source_id")] public string SourceId;

        [BsonElement("author_name")] public string AuthorName;

        [BsonElement("rating")] public string Rating;

        [BsonElement("date")] public string Date;

        [BsonElement("text")] public string Text;

        [BsonElement("is_readonly")] public bool IsReadOnly;
    }
}
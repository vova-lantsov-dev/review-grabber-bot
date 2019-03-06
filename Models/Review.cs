using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [BsonElement("comments")] public List<string> Comments;

        public override string ToString()
        {
            int.TryParse(Rating, out var ratingValue);
            
            var result = new StringBuilder();
            result.AppendFormat("*ОТЗЫВ*\n_{0} ({1})_", AuthorName, Date);
            if (ratingValue > 0)
            {
                result.Append("\nРейтинг: ");
                result.AppendJoin(string.Empty, Enumerable.Repeat("⭐️", ratingValue));
            }
            result.AppendFormat("\nРесторан: {0}\nИсточник: {1}\nТекст: {2}", RestaurantName, Resource, Text);
            
            return result.ToString();
        }
    }
}
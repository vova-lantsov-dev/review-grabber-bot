using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReviewGrabberBot.Formatters;

namespace ReviewGrabberBot.Models
{
    [BsonIgnoreExtraElements]
    internal sealed class Review
    {
        [BsonRepresentation(BsonType.ObjectId)] public string Id;
        
        [BsonElement("resource")] public string Resource;
        
        [BsonElement("restaurant_name")] public string RestaurantName;

        [BsonElement("need_to_show")] public bool NeedToShow;

        [BsonElement("reply_link")] public string ReplyLink;

        [BsonElement("author_name")] public string AuthorName;

        [BsonElement("author_avatar")] public string AuthorAvatar;

        [BsonElement("photos")] public List<string> Photos;

        [BsonElement("rating"), BsonSerializer(typeof(RatingMongoFormatter))] public int Rating;

        [BsonElement("date")] public string Date;

        [BsonElement("text")] public string Text;

        [BsonElement("is_readonly")] public bool IsReadOnly;

        [BsonElement("comments")] public List<string> Comments;

        [BsonElement("likes")] public int Likes;

        [BsonElement("dislikes")] public int Dislikes;

        [BsonElement("profile_link")] public string ProfileUrl;

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendFormat("{0} _({1})_", ProfileUrl == null
                ? $"*{AuthorName}*" : $"[{AuthorName}]({ProfileUrl})", Date);

            if (Rating > 0)
            {
                result.Append("\nРейтинг: ");
                result.AppendJoin(string.Empty, Enumerable.Repeat("⭐️", Rating));
            }

            if (Likes > 0)
            {
                result.AppendFormat("\n{0} {1}", Likes, Dislikes <= 0 ? "❤️" : "👍");

                if (Dislikes > 0)
                    result.AppendFormat("\n{0} 👎", Dislikes);
            }

            result.AppendFormat("\nРесторан: {0}\nИсточник: {1}\nТекст: {2}", RestaurantName, Resource, Text);
            return result.ToString();
        }
    }
}
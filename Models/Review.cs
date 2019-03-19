using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReviewGrabberBot.Formatters;
// ReSharper disable All
#pragma warning disable 649

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

        public string ToString(int maxCountOfStars, bool preferAvatarOverProfileLink)
        {
            var result = new StringBuilder();

            var link = !preferAvatarOverProfileLink ? ProfileUrl ?? AuthorAvatar : AuthorAvatar ?? ProfileUrl;
            result.AppendFormat("_Ресторан:_ *{0}*\n_Источник:_ *{1}*\n{2} _({3})_", RestaurantName, Resource,
                string.IsNullOrWhiteSpace(link) ? AuthorName : $"[{AuthorName}]({link})", Date);

            if (Rating > 0)
            {
                result.Append("\n_Рейтинг:_ ");
                result.AppendJoin(string.Empty, Enumerable.Repeat("👍", Rating));

                var emptyStarsCount = maxCountOfStars - Rating;
                if (emptyStarsCount > 0)
                    result.AppendJoin(string.Empty, Enumerable.Repeat("👍🏿", emptyStarsCount));
            }

            if (Likes > 0)
            {
                result.AppendFormat("\n{0} {1}", Likes, Dislikes <= 0 ? "❤️" : "👍");

                if (Dislikes > 0)
                    result.AppendFormat("\n{0} 👎", Dislikes);
            }

            if (!string.IsNullOrWhiteSpace(Text))
                result.AppendFormat("\n_Текст:_ {0}", Text);
            
            return result.ToString();
        }
    }
}
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewGrabberBot.Models
{
    internal sealed class GoogleCredential
    {
        [BsonElement] public string Id;
        
        [BsonElement("access_token")] public string AccessToken;
    }
}
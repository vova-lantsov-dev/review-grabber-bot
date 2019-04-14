using MongoDB.Bson.Serialization.Attributes;

namespace ReviewGrabberBot.Models
{
    [BsonIgnoreExtraElements]
    internal sealed class Credential
    {
        [BsonElement("name")] public string Name;
        
        [BsonElement("access_token")] public string AccessToken;
    }
}
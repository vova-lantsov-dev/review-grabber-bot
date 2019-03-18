using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Services
{
    internal sealed class Context
    {
        internal readonly IMongoCollection<Review> Reviews;
        internal readonly IMongoCollection<GoogleReviewMessage> GoogleReviewMessages;
        internal readonly IMongoCollection<GoogleCredential> GoogleCredentials;
        
        public Context(IOptions<ContextOptions> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);
            var db = mongoClient.GetDatabase(options.Value.DatabaseName);

            Reviews = db.GetCollection<Review>(options.Value.ReviewCollectionName);
            GoogleReviewMessages = db.GetCollection<GoogleReviewMessage>(nameof(GoogleReviewMessages));
            GoogleCredentials = db.GetCollection<GoogleCredential>("google_credentials");
        }
    }
}
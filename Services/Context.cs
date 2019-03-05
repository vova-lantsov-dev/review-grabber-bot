using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReviewGrabberBot.Options;

namespace ReviewGrabberBot.Services
{
    public sealed class Context
    {
        public Context(IOptions<ContextOptions> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);
            var db = mongoClient.GetDatabase(options.Value.DatabaseName);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;
using WomAWorldIntegration.Documents;

namespace WomAWorldIntegration
{
    public class MongoDatabase
    {
        private readonly MongoClient _client;
        private readonly ILogger<MongoDatabase> _logger;

        public MongoDatabase(
            MongoClient client,
            ILogger<MongoDatabase> logger
        )
        {
            _client = client;
            _logger = logger;
        }

        private IMongoDatabase Database
        {
            get
            {
                return _client.GetDatabase("AWorld");
            }
        }

        public IMongoCollection<User> UserCollection
        {
            get
            {
                return Database.GetCollection<User>("UserProfiles");
            }
        }

        public IMongoCollection<Prize> PrizeCollection
        {
            get
            {
                return Database.GetCollection<Prize>("AssignedPrizes");
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username.Trim());
            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary, caseLevel: false),
            };

            var result = await UserCollection.Find(filter, options).SingleOrDefaultAsync();
            _logger.LogDebug("Finding user with username {0} returns {1}", username, result);
            
            return result;
        }

        public async Task ReplaceUserByUsername(User user)
        {
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if(string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("User username cannot be empty or null", nameof(user.Username));
            }

            var filter = Builders<User>.Filter.Eq(u => u.Username, user.Username.Trim());
            var options = new ReplaceOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary, caseLevel: false),
                IsUpsert = true,
            };

            var result = await UserCollection.ReplaceOneAsync(filter, user, options);
            _logger.LogDebug("Replace user with username {0} matched {1} modified {2} upsert ID {3}",
                user.Username, result.MatchedCount, result.ModifiedCount, result.UpsertedId);
        }

        public async Task RegisterPrize(Prize prize)
        {
            await PrizeCollection.InsertOneAsync(prize);
        }

        public async Task<Prize> GetPrizeById(string prizeId)
        {
            if(!ObjectId.TryParse(prizeId, out var id))
            {
                return null;
            }

            return await PrizeCollection.Find(Builders<Prize>.Filter.Eq(p => p.Id, id)).SingleOrDefaultAsync();
        }

        public async Task<Prize> GetSafetyPrize(ObjectId userId)
        {
            var filter = Builders<Prize>.Filter.And(
                Builders<Prize>.Filter.Eq(p => p.UserId, userId),
                Builders<Prize>.Filter.Gt(p => p.CreatedOn, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
            );

            return await PrizeCollection.Find(filter).SortByDescending(p => p.CreatedOn).FirstOrDefaultAsync();
        }
    }
}

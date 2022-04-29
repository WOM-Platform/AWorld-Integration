using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WomAWorldIntegration.Documents
{
    public class Prize
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }

        public ObjectId UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        [BsonDefaultValue(false)]
        public bool NewRegistrationPrize { get; set; } = false;

        [BsonDefaultValue(0)]
        public int AmountOfLevelsGained { get; set; } = 0;

        [BsonDefaultValue(0)]
        public int AmountOfVouchers { get; set; } = 0;

        [BsonIgnoreIfNull]
        public string? WomUrl { get; set; }
        
        [BsonIgnoreIfNull]
        public string? WomPassword { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; } = new BsonDocument();
    }
}

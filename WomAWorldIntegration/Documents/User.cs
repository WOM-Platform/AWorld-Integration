using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WomAWorldIntegration.Documents
{
    public class User
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public Guid? Sub { get; set; }

        public DateTime CreatedOn { get; set; }

        public decimal SavedCo2 { get; set; }

        public decimal SavedWater { get; set; }

        public decimal SavedEnergy { get; set; }

        public decimal ActsOfLove { get; set; }

        public int ActionsCount { get; set; }

        public int CurrentLevelIndex { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; } = new BsonDocument();
    }
}

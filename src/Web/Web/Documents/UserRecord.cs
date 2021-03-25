using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Documents {

    public class UserRecord {

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("lastUpdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdate { get; set; }

        [BsonElement("metrics")]
        public Dictionary<int, double> Metrics { get; set; } = new Dictionary<int, double>();

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}

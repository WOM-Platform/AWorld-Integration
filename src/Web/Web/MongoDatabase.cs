using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Web {

    public class MongoDatabase {

        private readonly ILogger<MongoDatabase> _logger;

        public MongoDatabase(
            ILogger<MongoDatabase> logger) {
            _logger = logger;
        }

        private readonly object _lockRoot = new object();
        private MongoClient _client = null;

        private MongoClient Client {
            get {
                if(_client == null) {
                    lock(_lockRoot) {
                        if(_client == null) {
                            var connString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");

                            _logger.LogInformation("Creating new Mongo client");
                            _client = new MongoClient(connString);
                        }
                    }
                }

                return _client;
            }
        }

        

    }

}

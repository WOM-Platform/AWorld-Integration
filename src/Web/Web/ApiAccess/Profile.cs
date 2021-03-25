using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.ApiAccess {

    public class Profile {

        public DataSection Ctx { get; set; }

        public class DataSection {

            public string Username { get; set; }

            public string Lang { get; set; }

            public ProfileData Profile { get; set; }

        }

        public class ProfileData {

            public UserSection User { get; set; }

            public MetricEntry[] Metrics { get; set; }

            public int Activations { get; set; }

        }

        public class UserSection {

            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string LastName { get; set; }

            public string Sub { get; set; }

            public string Signature { get; set; }

        }

        public class MetricEntry {

            public int Id { get; set; }

            public string Name { get; set; }

            public double Amount { get; set; }

        }

    }

}

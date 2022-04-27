using System.Text.Json.Serialization;

namespace WomAWorldIntegration.Models
{
    public class UserProfile
    {
        public DataSection? Ctx { get; set; }

        public class DataSection
        {
            public string? Username { get; set; }

            public string? Lang { get; set; }

            public ProfileData? Profile { get; set; }
        }

        public class ProfileData
        {
            public UserSection? User { get; set; }

            public MetricEntry[]? Metrics { get; set; }

            public int Activations { get; set; }
        }

        public class UserSection
        {
            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string? LastName { get; set; }

            public Guid Sub { get; set; }

            public string? Signature { get; set; }
        }

        public class MetricEntry
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            [JsonPropertyName("real_amount")]
            public double Amount { get; set; }
        }
    }
}

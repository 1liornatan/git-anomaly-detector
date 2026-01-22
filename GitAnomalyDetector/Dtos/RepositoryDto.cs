using System.Text.Json.Serialization;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Dtos
{
    public class RepositoryDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("pushed_at")]
        [JsonConverter(typeof(FlexibleUnixTimestampConverter))]
        public long? PushedAt { get; set; }
    }
}

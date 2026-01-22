using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Dtos
{
    public class TeamDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}

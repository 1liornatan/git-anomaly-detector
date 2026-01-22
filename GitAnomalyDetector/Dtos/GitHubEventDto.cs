using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Dtos
{
    public class GitHubEventDto
    {
        [JsonPropertyName("event")]
        public string? Event { get; set; }

        [JsonPropertyName("payload")]
        public PayloadDto? Payload { get; set; }
    }
}

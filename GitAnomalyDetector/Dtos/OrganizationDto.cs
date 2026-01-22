using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Dtos
{
    public class OrganizationDto
    {
        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}

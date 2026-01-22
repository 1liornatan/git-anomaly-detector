using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Dtos
{
    public class UserDto
    {
        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}

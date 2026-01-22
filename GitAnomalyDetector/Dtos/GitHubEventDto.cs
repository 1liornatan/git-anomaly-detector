using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Dtos
{
    public class GitHubEventDto
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("team")]
        public TeamDto? Team { get; set; }

        [JsonPropertyName("organization")]
        public OrganizationDto? Organization { get; set; }

        [JsonPropertyName("sender")]
        public UserDto? Sender { get; set; }

        [JsonPropertyName("repository")]
        public RepositoryDto? Repository { get; set; }
    }
}

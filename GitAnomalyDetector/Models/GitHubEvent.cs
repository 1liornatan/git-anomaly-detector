namespace GitAnomalyDetector.Models
{
    public class GitHubEvent
    {
        public EventType Event { get; set; }

        public required Payload Payload { get; set; }
    }
}

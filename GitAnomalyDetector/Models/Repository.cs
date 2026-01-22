namespace GitAnomalyDetector.Models
{
    public class Repository
    {
        public required long Id { get; set; }

        public string? FullName { get; set; }

        public DateTime PushedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

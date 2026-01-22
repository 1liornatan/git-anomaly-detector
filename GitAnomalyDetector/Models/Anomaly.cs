namespace GitAnomalyDetector.Models
{
    public class Anomaly
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
    }
}

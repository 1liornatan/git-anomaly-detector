namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class Anomaly
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
    }
}

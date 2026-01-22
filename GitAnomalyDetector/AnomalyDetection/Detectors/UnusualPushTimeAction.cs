using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualPushTimeAction : IAnomalyDetectionAction
    {
        public const int SuspiciousTimeStart = 14;
        public const int SuspiciousTimeEnd = 16;
        public static string AnomalyType = "UnusualPushTime";
        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            var anomalies = new List<Anomaly>();
            var repository = gitHubEvent.Repository;

            if (gitHubEvent.Type != EventType.Push || repository == null)
            {
                return Task.FromResult(anomalies);
            }

            var pushTime = repository.PushedAt;

            if (pushTime.Hour >= SuspiciousTimeStart && pushTime.Hour <= SuspiciousTimeEnd)
            {
                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType,
                    Description = $"Push to repository '{repository.FullName}' occurred at an unusual time: {pushTime:HH:mm} UTC.",
                    Severity = SeverityLevel.Medium
                });
            }

            return Task.FromResult(anomalies);
        }
    }
}

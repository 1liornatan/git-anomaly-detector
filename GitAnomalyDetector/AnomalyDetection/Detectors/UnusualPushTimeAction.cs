using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualPushTimeAction : IAnomalyDetectionAction
    {
        private readonly ILogger<UnusualPushTimeAction> _logger;
        public const int SuspiciousTimeStart = 14;
        public const int SuspiciousTimeEnd = 16;
        public static string AnomalyType = "UnusualPushTime";

        public UnusualPushTimeAction(ILogger<UnusualPushTimeAction> logger)
        {
            _logger = logger;
        }
        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            _logger.LogDebug("UnusualPushTimeAction: Analyzing event");
            var anomalies = new List<Anomaly>();
            var repository = gitHubEvent.Repository;

            if (gitHubEvent.Type != EventType.Push || repository == null)
            {
                _logger.LogDebug("UnusualPushTimeAction: Event is not a push event or repository is null, skipping");
                return Task.FromResult(anomalies);
            }

            var pushTime = repository.PushedAt;
            _logger.LogDebug($"UnusualPushTimeAction: Checking push time {pushTime} for repository {repository.FullName}");

            if (pushTime.Hour >= SuspiciousTimeStart && pushTime.Hour <= SuspiciousTimeEnd)
            {
                _logger.LogWarning($"UnusualPushTimeAction: Detected unusual push time {pushTime} for repository {repository.FullName}");
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

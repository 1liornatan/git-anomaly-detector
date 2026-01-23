using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualRepositoryAccessAction : IAnomalyDetectionAction
    {
        private readonly ILogger<UnusualRepositoryAccessAction> _logger;
        private const int SuspiciousRepoDeletionTimeInDays = 10;
        public static string AnomalyType = "UnusualRepositoryAccess";

        public UnusualRepositoryAccessAction(ILogger<UnusualRepositoryAccessAction> logger)
        {
            _logger = logger;
        }

        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            _logger.LogDebug("UnusualRepositoryAccessAction: Analyzing event");
            var anomalies = new List<Anomaly>();

            if (gitHubEvent.Type != EventType.Repository || gitHubEvent.Action != EventAction.Deleted)
            {
                _logger.LogDebug("UnusualRepositoryAccessAction: Event is not a repository deletion event, skipping");
                return Task.FromResult(anomalies);
            }

            var repository = gitHubEvent.Repository;

            if (repository == null)
            {
                _logger.LogWarning("UnusualRepositoryAccessAction: Repository is null for deletion event");
                return Task.FromResult(anomalies);
            }

            var createdTime = repository.CreatedAt;
            var deletedTime = repository.PushedAt;
            var timeSpan = deletedTime - createdTime;
            _logger.LogDebug($"UnusualRepositoryAccessAction: Checking repository {repository.FullName} deletion. Created: {createdTime}, Deleted: {deletedTime}, Age: {timeSpan.TotalDays} days");

            if (deletedTime - createdTime <= TimeSpan.FromDays(SuspiciousRepoDeletionTimeInDays))
            {
                _logger.LogWarning($"UnusualRepositoryAccessAction: Detected suspicious repository deletion of {repository.FullName} within {timeSpan.TotalDays} days");
                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType,
                    Description = $"Removal of repository '{repository.FullName}' occurred less than {SuspiciousRepoDeletionTimeInDays} days after its creation.",
                    Severity = SeverityLevel.High
                });
            }

            return Task.FromResult(anomalies);
        }
    }
}

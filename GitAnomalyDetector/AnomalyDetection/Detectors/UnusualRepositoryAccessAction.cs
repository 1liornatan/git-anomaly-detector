using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualRepositoryAccessAction : IAnomalyDetectionAction
    {
        private const int SuspiciousRepoDeletionTimeInDays = 10;
        public static string AnomalyType = "UnusualRepositoryAccess";

        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            var anomalies = new List<Anomaly>();

            if (gitHubEvent.Type != EventType.Repository || gitHubEvent.Action != EventAction.Deleted)
            {
                return Task.FromResult(anomalies);
            }

            var repository = gitHubEvent.Repository;

            if (repository == null)
            {
                return Task.FromResult(anomalies);
            }

            var createdTime = repository.CreatedAt;
            var deletedTime = repository.PushedAt;

            if (deletedTime - createdTime <= TimeSpan.FromDays(SuspiciousRepoDeletionTimeInDays))
            {
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

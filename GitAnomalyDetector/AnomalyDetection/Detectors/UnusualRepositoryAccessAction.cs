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

            var payload = gitHubEvent.Payload;

            if (gitHubEvent.Event != EventType.Repository || payload.Action != EventAction.Deleted)
            {
                return Task.FromResult(anomalies);
            }

            var repository = payload.Repository;

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
                    Description = $"Push to repository '{repository.FullName}' occurred at an unusual time: {deletedTime:HH:mm} UTC.",
                    Severity = "High"
                });

                Console.WriteLine($"Detected unusual repository deletion for '{repository.FullName}' shortly after creation.");
            }

            return Task.FromResult(anomalies);
        }
    }
}

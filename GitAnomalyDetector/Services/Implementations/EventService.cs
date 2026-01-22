using GitAnomalyDetector.Common;
using GitAnomalyDetector.Models;
using GitAnomalyDetector.Services.AnomalyDetection;

namespace GitAnomalyDetector.Services
{
    public class EventService : IEventService
    {
        private readonly IEnumerable<IAnomalyDetectionAction> _anomalyDetectionActions;

        public EventService(IEnumerable<IAnomalyDetectionAction> anomalyDetectionActions)
        {
            _anomalyDetectionActions = anomalyDetectionActions;
        }

        public async Task<Result> HandleEventAsync(GitHubEvent gitHubEvent)
        {
            var allAnomalies = new List<Anomaly>();

            var detectionTasks = _anomalyDetectionActions.Select(action => action.DetectAsync(gitHubEvent));
            var results = await Task.WhenAll(detectionTasks);

            foreach (var anomalies in results)
            {
                allAnomalies.AddRange(anomalies);
            }

            string message = allAnomalies.Count > 0
                ? $"Event processed. {allAnomalies.Count} anomalie(s) detected."
                : "Event processed. No anomalies detected.";

            PrintAnomalies(allAnomalies);
            
            return Result.SuccessResult(message);
        }

        private void PrintAnomalies(List<Anomaly> allAnomalies)
        {
            allAnomalies.ForEach(anomaly =>
            {
                Console.WriteLine($"Anomaly Detected: Type={anomaly.Type}, Description={anomaly.Description}, Severity={anomaly.Severity}");
            });
        }
    }
}

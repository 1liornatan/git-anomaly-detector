using GitAnomalyDetector.Models;
using GitAnomalyDetector.Notifications;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Services
{
    public class EventService : IEventService
    {
        private readonly IEnumerable<IAnomalyDetectionAction> _anomalyDetectionActions;
        private readonly IEnumerable<INotificationAction> _notificationActions;

        public EventService(
            IEnumerable<IAnomalyDetectionAction> anomalyDetectionActions,
            IEnumerable<INotificationAction> notificationActions)
        {
            _anomalyDetectionActions = anomalyDetectionActions;
            _notificationActions = notificationActions;
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

            var notificationTasks = _notificationActions.Select(action => action.SendAsync(allAnomalies));
            await Task.WhenAll(notificationTasks);
            
            string message = allAnomalies.Count > 0
                ? $"Event processed. {allAnomalies.Count} anomalie(s) detected."
                : "Event processed. No anomalies detected.";
            
            return Result.SuccessResult(message);
        }
    }
}

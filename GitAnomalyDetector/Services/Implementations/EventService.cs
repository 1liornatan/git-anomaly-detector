using GitAnomalyDetector.Models;
using GitAnomalyDetector.Notifications;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Services
{
    public class EventService : IEventService
    {
        private readonly IEnumerable<IAnomalyDetectionAction> _anomalyDetectionActions;
        private readonly IEnumerable<INotificationAction> _notificationActions;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEnumerable<IAnomalyDetectionAction> anomalyDetectionActions,
            IEnumerable<INotificationAction> notificationActions,
            ILogger<EventService> logger)
        {
            _anomalyDetectionActions = anomalyDetectionActions;
            _notificationActions = notificationActions;
            _logger = logger;
        }

        public async Task<Result> HandleEventAsync(GitHubEvent gitHubEvent)
        {
            _logger.LogInformation($"Starting event processing for event type: {gitHubEvent.Type}");

            var allAnomalies = new List<Anomaly>();

            _logger.LogDebug($"Running {_anomalyDetectionActions.Count()} anomaly detection actions");
            var detectionTasks = _anomalyDetectionActions.Select(action => action.DetectAsync(gitHubEvent));
            var results = await Task.WhenAll(detectionTasks);

            foreach (var anomalies in results)
            {
                allAnomalies.AddRange(anomalies);
            }

            _logger.LogInformation($"Anomaly detection completed. Found {allAnomalies.Count} anomalies");

            if (allAnomalies.Count > 0)
            {
                _logger.LogDebug($"Sending notifications to {_notificationActions.Count()} channels");
                var notificationTasks = _notificationActions.Select(action => action.SendAsync(allAnomalies));
                await Task.WhenAll(notificationTasks);
            }

            string message = allAnomalies.Count > 0
                ? $"Event processed. {allAnomalies.Count} anomalie(s) detected."
                : "Event processed. No anomalies detected.";

            _logger.LogInformation(message);
            return Result.SuccessResult(message);
        }
    }
}

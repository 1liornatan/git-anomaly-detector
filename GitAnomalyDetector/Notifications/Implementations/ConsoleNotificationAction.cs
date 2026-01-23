using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Notifications.Implementations
{
    public class ConsoleNotificationAction : INotificationAction
    {
        private readonly ILogger<ConsoleNotificationAction> _logger;

        public ConsoleNotificationAction(ILogger<ConsoleNotificationAction> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(List<Anomaly> anomalies)
        {
            if (anomalies.Count == 0)
            {
                _logger.LogDebug("ConsoleNotificationAction: No anomalies to send");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"ConsoleNotificationAction: Sending {anomalies.Count} anomaly notifications");
            
            foreach (var anomaly in anomalies)
            {
                Console.WriteLine($"Anomaly Detected: Type={anomaly.Type}, Description={anomaly.Description}, Severity={anomaly.Severity}");
                _logger.LogInformation($"Anomaly sent - Type: {anomaly.Type}, Severity: {anomaly.Severity}, Description: {anomaly.Description}");
            }
            
            return Task.CompletedTask;
        }
    }
}

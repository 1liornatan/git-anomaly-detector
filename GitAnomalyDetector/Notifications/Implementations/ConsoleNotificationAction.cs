using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Notifications.Implementations
{
    public class ConsoleNotificationAction : INotificationAction
    {
        public Task SendAsync(List<Anomaly> anomalies)
        {
            if (anomalies.Count == 0)
                return Task.CompletedTask;
            
            foreach (var anomaly in anomalies)
            {
                Console.WriteLine($"Anomaly Detected: Type={anomaly.Type}, Description={anomaly.Description}, Severity={anomaly.Severity}");
            }
            
            return Task.CompletedTask;
        }
    }
}

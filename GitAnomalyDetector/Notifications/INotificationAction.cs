using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Notifications
{
    public interface INotificationAction
    {
        Task SendAsync(List<Anomaly> anomalies);
    }
}

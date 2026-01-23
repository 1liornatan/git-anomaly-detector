using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services
{
    public interface IAnomalyDetectionAction
    {
        Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent);
    }
}

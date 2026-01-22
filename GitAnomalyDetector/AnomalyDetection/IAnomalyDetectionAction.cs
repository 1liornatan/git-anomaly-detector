using GitAnomalyDetector.Models;
using GitAnomalyDetector.Services.AnomalyDetection;

namespace GitAnomalyDetector.Services
{
    public interface IAnomalyDetectionAction
    {
        Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent);
    }
}

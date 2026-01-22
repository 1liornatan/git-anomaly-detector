using GitAnomalyDetector.Models;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Services
{
    public interface IEventService
    {
        Task<Result> HandleEventAsync(GitHubEvent gitHubEvent);
    }
}

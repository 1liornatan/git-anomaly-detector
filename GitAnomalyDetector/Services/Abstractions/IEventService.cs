using GitAnomalyDetector.Common;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services
{
    public interface IEventService
    {
        Task<Result> HandleEventAsync(GitHubEvent gitHubEvent);
    }
}

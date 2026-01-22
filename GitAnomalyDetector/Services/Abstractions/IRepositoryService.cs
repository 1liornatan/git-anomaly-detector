using GitAnomalyDetector.Common;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services
{
    public interface IRepositoryService
    {
        Task<Result> ProcessRepositoryEventAsync(Repository repository);
    }
}

using GitAnomalyDetector.Common;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services
{
    public interface ITeamService
    {
        Task<Result> ProcessTeamEventAsync(Team team);
    }
}

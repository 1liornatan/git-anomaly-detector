using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualTeamNameAction : IAnomalyDetectionAction
    {
        private readonly ILogger<UnusualTeamNameAction> _logger;
        private const string SuspiciousNamePrefix = "hacker";
        public static string AnomalyType = "HackerPrefixTeam";

        public UnusualTeamNameAction(ILogger<UnusualTeamNameAction> logger)
        {
            _logger = logger;
        }

        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            _logger.LogDebug("UnusualTeamNameAction: Analyzing event");
            var anomalies = new List<Anomaly>();


            if (gitHubEvent.Type != EventType.Team || gitHubEvent.Action != EventAction.Created)
            {
                _logger.LogDebug("UnusualTeamNameAction: Event is not a team creation event, skipping");
                return Task.FromResult(anomalies);
            }

            var team = gitHubEvent.Team;

            if (string.IsNullOrWhiteSpace(team?.Name))
            {
                _logger.LogWarning("UnusualTeamNameAction: Team name is null or empty for team creation event");
                return Task.FromResult(anomalies);
            }

            _logger.LogDebug($"UnusualTeamNameAction: Checking team name {team.Name} for suspicious prefix");

            if (team.Name.StartsWith(SuspiciousNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"UnusualTeamNameAction: Detected suspicious team name {team.Name} with '{SuspiciousNamePrefix}' prefix");
                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType,
                    Description = $"Team '{team.Name}' created with suspicious '{SuspiciousNamePrefix}' prefix.",
                    Severity = SeverityLevel.Critical
                });
            }

            return Task.FromResult(anomalies);
        }
    }
}

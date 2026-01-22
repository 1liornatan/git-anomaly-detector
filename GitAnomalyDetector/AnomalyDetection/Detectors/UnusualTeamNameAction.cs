using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Services.AnomalyDetection
{
    public class UnusualTeamNameAction : IAnomalyDetectionAction
    {
        private const string SuspiciousNamePrefix = "hacker";
        public static string AnomalyType = "HackerPrefixTeam";

        public Task<List<Anomaly>> DetectAsync(GitHubEvent gitHubEvent)
        {
            var anomalies = new List<Anomaly>();


            if (gitHubEvent.Type != EventType.Team || gitHubEvent.Action != EventAction.Created)
            {
                return Task.FromResult(anomalies);
            }

            var team = gitHubEvent.Team;

            if (string.IsNullOrWhiteSpace(team?.Name))
            {
                return Task.FromResult(anomalies);
            }

            if (team.Name.StartsWith(SuspiciousNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
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

using GitAnomalyDetector.Models;
using GitAnomalyDetector.Services.AnomalyDetection;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    public class UnusualTeamNameActionTests
    {
        private readonly UnusualTeamNameAction _detector;

        public UnusualTeamNameActionTests()
        {
            _detector = new UnusualTeamNameAction();
        }

        [Theory]
        [InlineData("hackerteam")]      // Lowercase
        [InlineData("HackerGroup")]     // Capitalized
        [InlineData("HaCkErElite")]     // Mixed case
        [InlineData("HACKERS")]         // Uppercase
        public async Task DetectAsync_TeamCreatedWithHackerPrefix_ReturnsAnomaly(string teamName)
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Action = EventAction.Created,
                Team = new Team
                {
                    Id = 789,
                    Name = teamName
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Single(result);
            Assert.Equal("HackerPrefixTeam", result[0].Type);
            Assert.Equal(SeverityLevel.Critical, result[0].Severity);
            Assert.Contains(teamName, result[0].Description);
            Assert.Contains("suspicious", result[0].Description);
        }

        [Theory]
        [InlineData("developers")]      // Normal name
        [InlineData("growthackers")]    // Hacker in middle
        public async Task DetectAsync_TeamCreatedWithNormalName_ReturnsNoAnomaly(string teamName)
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Action = EventAction.Created,
                Team = new Team
                {
                    Id = 789,
                    Name = teamName
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DetectAsync_TeamDeletedEvent_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Action = EventAction.Deleted,
                Team = new Team
                {
                    Id = 789,
                    Name = "hackerteam"
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DetectAsync_NonTeamEvent_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Push,
                Action = EventAction.Created,
                Team = new Team
                {
                    Id = 789,
                    Name = "hackerteam"
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(null)]        // Null name
        [InlineData("")]          // Empty name
        [InlineData("   ")]       // Whitespace name
        public async Task DetectAsync_TeamWithInvalidName_ReturnsNoAnomaly(string? teamName)
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Action = EventAction.Created,
                Team = new Team
                {
                    Id = 789,
                    Name = teamName
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DetectAsync_NullTeam_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Action = EventAction.Created,
                Team = null
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }
    }
}

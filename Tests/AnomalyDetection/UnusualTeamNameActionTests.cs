using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitAnomalyDetector.Services.AnomalyDetection;
using GitAnomalyDetector.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    [TestClass]
    public class UnusualTeamNameActionTests
    {
        private UnusualTeamNameAction _detector;

        [TestInitialize]
        public void Initialize()
        {
            var mockLogger = new Mock<ILogger<UnusualTeamNameAction>>();
            _detector = new UnusualTeamNameAction(mockLogger.Object);
        }

        [DataTestMethod]
        [DataRow("hackerteam")]      // Lowercase
        [DataRow("HackerGroup")]     // Capitalized
        [DataRow("HaCkErElite")]     // Mixed case
        [DataRow("HACKERS")]         // Uppercase
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
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("HackerPrefixTeam", result[0].Type);
            Assert.AreEqual(SeverityLevel.Critical, result[0].Severity);
            Assert.IsTrue(result[0].Description.Contains(teamName));
            Assert.IsTrue(result[0].Description.Contains("suspicious"));
        }

        [DataTestMethod]
        [DataRow("developers")]      // Normal name
        [DataRow("growthackers")]    // Hacker in middle
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
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
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
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
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
            Assert.AreEqual(0, result.Count());
        }

        [DataTestMethod]
        [DataRow(null)]        // Null name
        [DataRow("")]          // Empty name
        [DataRow("   ")]       // Whitespace name
        public async Task DetectAsync_TeamWithInvalidName_ReturnsNoAnomaly(string teamName)
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
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
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
            Assert.AreEqual(0, result.Count());
        }
    }
}

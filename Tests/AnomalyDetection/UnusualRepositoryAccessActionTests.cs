using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitAnomalyDetector.Services.AnomalyDetection;
using GitAnomalyDetector.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    [TestClass]
    public class UnusualRepositoryAccessActionTests
    {
        private UnusualRepositoryAccessAction _detector;

        [TestInitialize]
        public void Initialize()
        {
            var mockLogger = new Mock<ILogger<UnusualRepositoryAccessAction>>();
            _detector = new UnusualRepositoryAccessAction(mockLogger.Object);
        }

        [DataTestMethod]
        [DataRow(4)]   // Within threshold
        [DataRow(10)]  // At boundary
        [DataRow(0)]   // Immediate deletion
        public async Task DetectAsync_RepositoryDeletedWithin10Minutes_ReturnsAnomaly(int minutesAfterCreation)
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var deletedTime = createdTime.AddMinutes(minutesAfterCreation);
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Repository,
                Action = EventAction.Deleted,
                Repository = new Repository
                {
                    Id = 456,
                    FullName = "test/suspicious-repo",
                    CreatedAt = createdTime,
                    PushedAt = deletedTime
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("UnusualRepositoryAccess", result[0].Type);
            Assert.AreEqual(SeverityLevel.High, result[0].Severity);
            Assert.IsTrue(result[0].Description.Contains("test/suspicious-repo"));
            Assert.IsTrue(result[0].Description.Contains("10 minutes"));
        }

        [DataTestMethod]
        [DataRow(11)]  // Just after threshold
        [DataRow(30)]  // Much later
        public async Task DetectAsync_RepositoryDeletedAfter10Minutes_ReturnsNoAnomaly(int minutesAfterCreation)
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var deletedTime = createdTime.AddMinutes(minutesAfterCreation);
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Repository,
                Action = EventAction.Deleted,
                Repository = new Repository
                {
                    Id = 456,
                    FullName = "test/normal-repo",
                    CreatedAt = createdTime,
                    PushedAt = deletedTime
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DetectAsync_RepositoryCreatedEvent_ReturnsNoAnomaly()
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Repository,
                Action = EventAction.Created,
                Repository = new Repository
                {
                    Id = 456,
                    FullName = "test/new-repo",
                    CreatedAt = createdTime,
                    PushedAt = createdTime
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DetectAsync_NonRepositoryEvent_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Push,
                Action = EventAction.Deleted,
                Repository = new Repository
                {
                    Id = 456,
                    FullName = "test/repo",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    PushedAt = DateTime.UtcNow
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DetectAsync_NullRepository_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Repository,
                Action = EventAction.Deleted,
                Repository = null
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }
    }
}

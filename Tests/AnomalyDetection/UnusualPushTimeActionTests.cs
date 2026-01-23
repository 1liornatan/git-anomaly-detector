using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitAnomalyDetector.Services.AnomalyDetection;
using GitAnomalyDetector.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    [TestClass]
    public class UnusualPushTimeActionTests
    {
        private UnusualPushTimeAction _detector;

        [TestInitialize]
        public void Initialize()
        {
            var mockLogger = new Mock<ILogger<UnusualPushTimeAction>>();
            _detector = new UnusualPushTimeAction(mockLogger.Object);
        }

        [DataTestMethod]
        [DataRow(14, 0)]  // Start boundary
        [DataRow(15, 30)] // Middle
        [DataRow(16, 0)]  // End boundary
        public async Task DetectAsync_PushEventDuringSuspiciousHours_ReturnsAnomaly(int hour, int minute)
        {
            // Arrange
            var suspiciousTime = new DateTime(2026, 1, 23, hour, minute, 0, DateTimeKind.Utc);
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Push,
                Repository = new Repository
                {
                    Id = 123,
                    FullName = "test/repo",
                    PushedAt = suspiciousTime,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("UnusualPushTime", result[0].Type);
            Assert.AreEqual(SeverityLevel.Medium, result[0].Severity);
            Assert.IsTrue(result[0].Description.Contains("test/repo"));
        }

        [DataTestMethod]
        [DataRow(10, 0)]  // Normal hours
        [DataRow(13, 59)] // Just before range
        [DataRow(17, 0)]  // Just after range
        public async Task DetectAsync_PushEventOutsideSuspiciousHours_ReturnsNoAnomaly(int hour, int minute)
        {
            // Arrange
            var normalTime = new DateTime(2026, 1, 23, hour, minute, 0, DateTimeKind.Utc);
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Push,
                Repository = new Repository
                {
                    Id = 123,
                    FullName = "test/repo",
                    PushedAt = normalTime,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DetectAsync_NonPushEvent_ReturnsNoAnomaly()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent
            {
                Type = EventType.Team,
                Repository = new Repository
                {
                    Id = 123,
                    FullName = "test/repo",
                    PushedAt = new DateTime(2026, 1, 23, 15, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
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
                Type = EventType.Push,
                Repository = null
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.AreEqual(0, result.Count());
        }
    }
}

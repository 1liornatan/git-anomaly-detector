using GitAnomalyDetector.Models;
using GitAnomalyDetector.Services.AnomalyDetection;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    public class UnusualPushTimeActionTests
    {
        private readonly UnusualPushTimeAction _detector;

        public UnusualPushTimeActionTests()
        {
            _detector = new UnusualPushTimeAction();
        }

        [Theory]
        [InlineData(14, 0)]  // Start boundary
        [InlineData(15, 30)] // Middle
        [InlineData(16, 0)]  // End boundary
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
            Assert.Single(result);
            Assert.Equal("UnusualPushTime", result[0].Type);
            Assert.Equal(SeverityLevel.Medium, result[0].Severity);
            Assert.Contains("test/repo", result[0].Description);
        }

        [Theory]
        [InlineData(10, 0)]  // Normal hours
        [InlineData(13, 59)] // Just before range
        [InlineData(17, 0)]  // Just after range
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
            Assert.Empty(result);
        }

        [Fact]
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
            Assert.Empty(result);
        }

        [Fact]
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
            Assert.Empty(result);
        }
    }
}

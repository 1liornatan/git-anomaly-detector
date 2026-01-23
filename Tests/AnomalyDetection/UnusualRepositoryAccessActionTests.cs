using GitAnomalyDetector.Models;
using GitAnomalyDetector.Services.AnomalyDetection;

namespace GitAnomalyDetector.Tests.AnomalyDetection
{
    public class UnusualRepositoryAccessActionTests
    {
        private readonly UnusualRepositoryAccessAction _detector;

        public UnusualRepositoryAccessActionTests()
        {
            _detector = new UnusualRepositoryAccessAction();
        }

        [Theory]
        [InlineData(4)]   // Within threshold
        [InlineData(10)]  // At boundary
        [InlineData(0)]   // Immediate deletion
        public async Task DetectAsync_RepositoryDeletedWithin10Days_ReturnsAnomaly(int daysAfterCreation)
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var deletedTime = createdTime.AddDays(daysAfterCreation).AddMinutes(5);
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
            Assert.Single(result);
            Assert.Equal("UnusualRepositoryAccess", result[0].Type);
            Assert.Equal(SeverityLevel.High, result[0].Severity);
            Assert.Contains("test/suspicious-repo", result[0].Description);
            Assert.Contains("10 days", result[0].Description);
        }

        [Theory]
        [InlineData(11)]  // Just after threshold
        [InlineData(30)]  // Much later
        public async Task DetectAsync_RepositoryDeletedAfter10Days_ReturnsNoAnomaly(int daysAfterCreation)
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var deletedTime = createdTime.AddDays(daysAfterCreation);
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
            Assert.Empty(result);
        }

        [Fact]
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
            Assert.Empty(result);
        }

        [Fact]
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
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    PushedAt = DateTime.UtcNow
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
                Type = EventType.Repository,
                Action = EventAction.Deleted,
                Repository = null
            };

            // Act
            var result = await _detector.DetectAsync(gitHubEvent);

            // Assert
            Assert.Empty(result);
        }
    }
}

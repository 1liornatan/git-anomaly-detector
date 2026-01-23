using GitAnomalyDetector.Models;
using GitAnomalyDetector.Notifications;
using GitAnomalyDetector.Services;
using GitAnomalyDetector.Utils;
using Moq;

namespace GitAnomalyDetector.Tests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IAnomalyDetectionAction> _mockDetector1;
        private readonly Mock<IAnomalyDetectionAction> _mockDetector2;
        private readonly Mock<INotificationAction> _mockNotification1;
        private readonly Mock<INotificationAction> _mockNotification2;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _mockDetector1 = new Mock<IAnomalyDetectionAction>();
            _mockDetector2 = new Mock<IAnomalyDetectionAction>();
            _mockNotification1 = new Mock<INotificationAction>();
            _mockNotification2 = new Mock<INotificationAction>();

            var detectors = new List<IAnomalyDetectionAction> { _mockDetector1.Object, _mockDetector2.Object };
            var notifications = new List<INotificationAction> { _mockNotification1.Object, _mockNotification2.Object };

            _eventService = new EventService(detectors, notifications);
        }

        [Fact]
        public async Task HandleEventAsync_WithNoAnomalies_ReturnsSuccessWithNoAnomaliesMessage()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Push };
            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());

            // Act
            var result = await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Event processed. No anomalies detected.", result.Message);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
            _mockDetector2.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
            _mockNotification1.Verify(n => n.SendAsync(It.IsAny<List<Anomaly>>()), Times.Once);
            _mockNotification2.Verify(n => n.SendAsync(It.IsAny<List<Anomaly>>()), Times.Once);
        }

        [Fact]
        public async Task HandleEventAsync_WithSingleAnomaly_ReturnsSuccessWithCountMessage()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Push };
            var anomaly = new Anomaly 
            { 
                Type = "TestAnomaly", 
                Description = "Test", 
                Severity = SeverityLevel.Medium 
            };

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly> { anomaly });
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());

            // Act
            var result = await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Event processed. 1 anomalie(s) detected.", result.Message);
        }

        [Fact]
        public async Task HandleEventAsync_WithMultipleAnomalies_ReturnsSuccessWithCorrectCount()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Team };
            var anomaly1 = new Anomaly 
            { 
                Type = "TestAnomaly1", 
                Description = "Test1", 
                Severity = SeverityLevel.High 
            };
            var anomaly2 = new Anomaly 
            { 
                Type = "TestAnomaly2", 
                Description = "Test2", 
                Severity = SeverityLevel.Critical 
            };
            var anomaly3 = new Anomaly 
            { 
                Type = "TestAnomaly3", 
                Description = "Test3", 
                Severity = SeverityLevel.Low 
            };

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly> { anomaly1, anomaly2 });
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly> { anomaly3 });

            // Act
            var result = await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Event processed. 3 anomalie(s) detected.", result.Message);
        }

        [Fact]
        public async Task HandleEventAsync_CallsAllDetectors_InParallel()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Repository };
            var detector1Called = false;
            var detector2Called = false;

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent))
                .ReturnsAsync(() =>
                {
                    detector1Called = true;
                    return new List<Anomaly>();
                });

            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent))
                .ReturnsAsync(() =>
                {
                    detector2Called = true;
                    return new List<Anomaly>();
                });

            // Act
            await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(detector1Called);
            Assert.True(detector2Called);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
            _mockDetector2.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
        }

        [Fact]
        public async Task HandleEventAsync_CallsAllNotifications_WithCorrectAnomalies()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Push };
            var anomaly = new Anomaly 
            { 
                Type = "TestAnomaly", 
                Description = "Test", 
                Severity = SeverityLevel.Medium 
            };

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly> { anomaly });
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());

            List<Anomaly>? capturedAnomalies1 = null;
            List<Anomaly>? capturedAnomalies2 = null;

            _mockNotification1.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies1 = anomalies)
                .Returns(Task.CompletedTask);

            _mockNotification2.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies2 = anomalies)
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.NotNull(capturedAnomalies1);
            Assert.NotNull(capturedAnomalies2);
            Assert.Single(capturedAnomalies1);
            Assert.Single(capturedAnomalies2);
            Assert.Equal("TestAnomaly", capturedAnomalies1[0].Type);
            Assert.Equal("TestAnomaly", capturedAnomalies2[0].Type);
        }

        [Fact]
        public async Task HandleEventAsync_WithNoDetectors_ReturnsSuccessWithNoAnomalies()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Ping };
            var emptyDetectors = new List<IAnomalyDetectionAction>();
            var notifications = new List<INotificationAction> { _mockNotification1.Object };
            var eventService = new EventService(emptyDetectors, notifications);

            // Act
            var result = await eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Event processed. No anomalies detected.", result.Message);
        }

        [Fact]
        public async Task HandleEventAsync_WithNoNotifications_StillProcessesDetectors()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Push };
            var anomaly = new Anomaly 
            { 
                Type = "TestAnomaly", 
                Description = "Test", 
                Severity = SeverityLevel.Low 
            };

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly> { anomaly });

            var detectors = new List<IAnomalyDetectionAction> { _mockDetector1.Object };
            var emptyNotifications = new List<INotificationAction>();
            var eventService = new EventService(detectors, emptyNotifications);

            // Act
            var result = await eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Event processed. 1 anomalie(s) detected.", result.Message);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
        }

        [Fact]
        public async Task HandleEventAsync_AggregatesAnomaliesFromAllDetectors()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Team };
            var anomaly1 = new Anomaly { Type = "Type1", Description = "Desc1", Severity = SeverityLevel.Low };
            var anomaly2 = new Anomaly { Type = "Type2", Description = "Desc2", Severity = SeverityLevel.Medium };
            var anomaly3 = new Anomaly { Type = "Type3", Description = "Desc3", Severity = SeverityLevel.High };

            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent))
                .ReturnsAsync(new List<Anomaly> { anomaly1, anomaly2 });
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent))
                .ReturnsAsync(new List<Anomaly> { anomaly3 });

            List<Anomaly>? capturedAnomalies = null;
            _mockNotification1.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies = anomalies)
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.NotNull(capturedAnomalies);
            Assert.Equal(3, capturedAnomalies.Count);
            Assert.Contains(capturedAnomalies, a => a.Type == "Type1");
            Assert.Contains(capturedAnomalies, a => a.Type == "Type2");
            Assert.Contains(capturedAnomalies, a => a.Type == "Type3");
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GitAnomalyDetector.Models;
using GitAnomalyDetector.Notifications;
using GitAnomalyDetector.Services;
using GitAnomalyDetector.Services.AnomalyDetection;
using Microsoft.Extensions.Logging;

namespace GitAnomalyDetector.Tests.Services
{
    [TestClass]
    public class EventServiceTests
    {
        private Mock<IAnomalyDetectionAction> _mockDetector1;
        private Mock<IAnomalyDetectionAction> _mockDetector2;
        private Mock<INotificationAction> _mockNotification1;
        private Mock<INotificationAction> _mockNotification2;
        private EventService _eventService;

        [TestInitialize]
        public void Initialize()
        {
            _mockDetector1 = new Mock<IAnomalyDetectionAction>();
            _mockDetector2 = new Mock<IAnomalyDetectionAction>();
            _mockNotification1 = new Mock<INotificationAction>();
            _mockNotification2 = new Mock<INotificationAction>();

            var detectors = new List<IAnomalyDetectionAction> { _mockDetector1.Object, _mockDetector2.Object };
            var notifications = new List<INotificationAction> { _mockNotification1.Object, _mockNotification2.Object };
            var mockLogger = new Mock<ILogger<EventService>>();

            _eventService = new EventService(detectors, notifications, mockLogger.Object);
        }

        [TestMethod]
        public async Task HandleEventAsync_WithNoAnomalies_ReturnsSuccessWithNoAnomaliesMessage()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Push };
            _mockDetector1.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());
            _mockDetector2.Setup(d => d.DetectAsync(gitHubEvent)).ReturnsAsync(new List<Anomaly>());

            // Act
            var result = await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Event processed. No anomalies detected.", result.Message);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
            _mockDetector2.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
        }

        [TestMethod]
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Event processed. 1 anomalie(s) detected.", result.Message);
        }

        [TestMethod]
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Event processed. 3 anomalie(s) detected.", result.Message);
        }

        [TestMethod]
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
            Assert.IsTrue(detector1Called);
            Assert.IsTrue(detector2Called);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
            _mockDetector2.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
        }

        [TestMethod]
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

            List<Anomaly> capturedAnomalies1 = null;
            List<Anomaly> capturedAnomalies2 = null;

            _mockNotification1.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies1 = anomalies)
                .Returns(Task.CompletedTask);

            _mockNotification2.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies2 = anomalies)
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.IsNotNull(capturedAnomalies1);
            Assert.IsNotNull(capturedAnomalies2);
            Assert.AreEqual(1, capturedAnomalies1.Count());
            Assert.AreEqual(1, capturedAnomalies2.Count());
            Assert.AreEqual("TestAnomaly", capturedAnomalies1[0].Type);
            Assert.AreEqual("TestAnomaly", capturedAnomalies2[0].Type);
        }

        [TestMethod]
        public async Task HandleEventAsync_WithNoDetectors_ReturnsSuccessWithNoAnomalies()
        {
            // Arrange
            var gitHubEvent = new GitHubEvent { Type = EventType.Ping };
            var emptyDetectors = new List<IAnomalyDetectionAction>();
            var notifications = new List<INotificationAction> { _mockNotification1.Object };
            var mockLogger = new Mock<ILogger<EventService>>();
            var eventService = new EventService(emptyDetectors, notifications, mockLogger.Object);

            // Act
            var result = await eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Event processed. No anomalies detected.", result.Message);
        }

        [TestMethod]
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
            var mockLogger = new Mock<ILogger<EventService>>();
            var eventService = new EventService(detectors, emptyNotifications, mockLogger.Object);

            // Act
            var result = await eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Event processed. 1 anomalie(s) detected.", result.Message);
            _mockDetector1.Verify(d => d.DetectAsync(gitHubEvent), Times.Once);
        }

        [TestMethod]
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

            List<Anomaly> capturedAnomalies = null;
            _mockNotification1.Setup(n => n.SendAsync(It.IsAny<List<Anomaly>>()))
                .Callback<List<Anomaly>>(anomalies => capturedAnomalies = anomalies)
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.HandleEventAsync(gitHubEvent);

            // Assert
            Assert.IsNotNull(capturedAnomalies);
            Assert.AreEqual(3, capturedAnomalies.Count);
            Assert.IsTrue(capturedAnomalies.Any(a => a.Type == "Type1"));
            Assert.IsTrue(capturedAnomalies.Any(a => a.Type == "Type2"));
            Assert.IsTrue(capturedAnomalies.Any(a => a.Type == "Type3"));
        }
    }
}

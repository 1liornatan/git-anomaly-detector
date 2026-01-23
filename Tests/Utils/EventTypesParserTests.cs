using GitAnomalyDetector.Controllers;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Tests.Utils
{
    public class EventTypesParserTests
    {
        [Theory]
        [InlineData("Push", EventType.Push)]
        [InlineData("Team", EventType.Team)]
        [InlineData("Repository", EventType.Repository)]
        [InlineData("Ping", EventType.Ping)]
        public void ParseEventType_ValidType_ReturnsCorrectEventType(string input, EventType expected)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("push")]         // Lowercase
        [InlineData("TEAM")]         // Uppercase
        [InlineData("rEpOsItOrY")]   // Mixed case
        public void ParseEventType_CaseInsensitiveInput_ReturnsCorrectType(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.NotEqual(EventType.Unknown, result);
        }

        [Theory]
        [InlineData("InvalidType")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseEventType_InvalidInput_ReturnsUnknown(string? input)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.Equal(EventType.Unknown, result);
        }

        [Theory]
        [InlineData("Created", EventAction.Created)]
        [InlineData("Deleted", EventAction.Deleted)]
        public void ParseEventAction_ValidAction_ReturnsCorrectEventAction(string input, EventAction expected)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("created")]      // Lowercase
        [InlineData("DELETED")]      // Uppercase
        [InlineData("CrEaTeD")]      // Mixed case
        public void ParseEventAction_CaseInsensitiveInput_ReturnsCorrectAction(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.NotEqual(EventAction.Unknown, result);
        }

        [Theory]
        [InlineData("InvalidAction")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseEventAction_InvalidInput_ReturnsUnknown(string? input)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.Equal(EventAction.Unknown, result);
        }
    }
}

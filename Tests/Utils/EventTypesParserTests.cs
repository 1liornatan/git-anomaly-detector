using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitAnomalyDetector.Models;
using GitAnomalyDetector.Utils;
using GitAnomalyDetector.Controllers;

namespace GitAnomalyDetector.Tests.Utils
{
    [TestClass]
    public class EventTypesParserTests
    {
        [DataTestMethod]
        [DataRow("Push", EventType.Push)]
        [DataRow("Team", EventType.Team)]
        [DataRow("Repository", EventType.Repository)]
        [DataRow("Ping", EventType.Ping)]
        public void ParseEventType_ValidType_ReturnsCorrectEventType(string input, EventType expected)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("push")]         // Lowercase
        [DataRow("TEAM")]         // Uppercase
        [DataRow("rEpOsItOrY")]   // Mixed case
        public void ParseEventType_CaseInsensitiveInput_ReturnsCorrectType(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.AreNotEqual(EventType.Unknown, result);
        }

        [DataTestMethod]
        [DataRow("InvalidType")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void ParseEventType_InvalidInput_ReturnsUnknown(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventType(input);

            // Assert
            Assert.AreEqual(EventType.Unknown, result);
        }

        [DataTestMethod]
        [DataRow("Created", EventAction.Created)]
        [DataRow("Deleted", EventAction.Deleted)]
        public void ParseEventAction_ValidAction_ReturnsCorrectEventAction(string input, EventAction expected)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("created")]      // Lowercase
        [DataRow("DELETED")]      // Uppercase
        [DataRow("CrEaTeD")]      // Mixed case
        public void ParseEventAction_CaseInsensitiveInput_ReturnsCorrectAction(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.AreNotEqual(EventAction.Unknown, result);
        }

        [DataTestMethod]
        [DataRow("InvalidAction")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void ParseEventAction_InvalidInput_ReturnsUnknown(string input)
        {
            // Act
            var result = EventTypesParser.ParseEventAction(input);

            // Assert
            Assert.AreEqual(EventAction.Unknown, result);
        }
    }
}

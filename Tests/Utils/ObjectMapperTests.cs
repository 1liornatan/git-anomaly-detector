using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitAnomalyDetector.Dtos;
using GitAnomalyDetector.Models;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Tests.Utils
{
    [TestClass]
    public class ObjectMapperTests
    {
        [TestMethod]
        public void MapTeam_ValidTeamDto_ReturnsTeam()
        {
            // Arrange
            var teamDto = new TeamDto
            {
                Id = 123,
                Name = "TestTeam"
            };

            // Act
            var result = ObjectMapper.MapTeam(teamDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(123, result.Id);
            Assert.AreEqual("TestTeam", result.Name);
        }

        [TestMethod]
        public void MapTeam_NullTeamDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapTeam(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void MapRepository_ValidRepositoryDto_ReturnsRepository()
        {
            // Arrange
            var repositoryDto = new RepositoryDto
            {
                Id = 789,
                FullName = "owner/repo",
                PushedAt = 1640000000, // Unix timestamp
                CreatedAt = 1630000000  // Unix timestamp
            };

            // Act
            var result = ObjectMapper.MapRepository(repositoryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(789, result.Id);
            Assert.AreEqual("owner/repo", result.FullName);
            Assert.AreEqual(new DateTime(2021, 12, 20, 11, 33, 20, DateTimeKind.Utc), result.PushedAt);
            Assert.AreEqual(new DateTime(2021, 8, 26, 17, 46, 40, DateTimeKind.Utc), result.CreatedAt);
        }

        [TestMethod]
        public void MapRepository_NullTimestamps_UsesMinValue()
        {
            // Arrange
            var repositoryDto = new RepositoryDto
            {
                Id = 789,
                FullName = "owner/repo",
                PushedAt = null,
                CreatedAt = null
            };

            // Act
            var result = ObjectMapper.MapRepository(repositoryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.MinValue, result.PushedAt);
            Assert.AreEqual(DateTime.MinValue, result.CreatedAt);
        }

        [TestMethod]
        public void MapRepository_NullRepositoryDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapRepository(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void MapOrganization_ValidOrganizationDto_ReturnsOrganization()
        {
            // Arrange
            var organizationDto = new OrganizationDto
            {
                Id = 111,
                Login = "test-org"
            };

            // Act
            var result = ObjectMapper.MapOrganization(organizationDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(111, result.Id);
            Assert.AreEqual("test-org", result.Login);
        }

        [DataTestMethod]
        [DataRow(null)]    // Null DTO
        [DataRow("")]      // Null login (empty string used as marker)
        public void MapOrganization_InvalidInput_ReturnsNull(string marker)
        {
            // Arrange
            OrganizationDto organizationDto = marker == null 
                ? null 
                : new OrganizationDto { Id = 222, Login = null };

            // Act
            var result = ObjectMapper.MapOrganization(organizationDto);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void MapUser_ValidUserDto_ReturnsUser()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 333,
                Login = "test-user"
            };

            // Act
            var result = ObjectMapper.MapUser(userDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(333, result.Id);
            Assert.AreEqual("test-user", result.Login);
        }

        [TestMethod]
        public void MapUser_NullUserDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapUser(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void MapUser_UserDtoWithNullLogin_ReturnsUserWithEmptyString()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 444,
                Login = null
            };

            // Act
            var result = ObjectMapper.MapUser(userDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(444, result.Id);
            Assert.AreEqual(string.Empty, result.Login);
        }

        [DataTestMethod]
        [DataRow(1640000000, 2021, 12, 20, 11, 33, 20)]  // Standard timestamp
        [DataRow(0, 1970, 1, 1, 0, 0, 0)]                 // Unix epoch
        [DataRow(1800000000, 2027, 1, 15, 8, 0, 0)]       // Future timestamp
        public void UnixTimeStampToDateTime_ValidTimestamp_ReturnsCorrectDateTime(
            long timestamp, int year, int month, int day, int hour, int minute, int second)
        {
            // Act
            var result = ObjectMapper.UnixTimeStampToDateTime(timestamp);

            // Assert
            Assert.AreEqual(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc), result);
        }

        [TestMethod]
        public void MapGitHubEvent_CompleteGitHubEventDto_ReturnsCompleteGitHubEvent()
        {
            // Arrange
            var gitHubEventDto = new GitHubEventDto
            {
                Action = "Created",
                Team = new TeamDto { Id = 123, Name = "TestTeam" },
                Organization = new OrganizationDto { Id = 456, Login = "TestOrg" },
                Sender = new UserDto { Id = 789, Login = "TestUser" },
                Repository = new RepositoryDto 
                { 
                    Id = 111, 
                    FullName = "owner/repo",
                    PushedAt = 1640000000,
                    CreatedAt = 1630000000
                }
            };

            // Act
            var result = ObjectMapper.MapGitHubEvent(gitHubEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(EventAction.Created, result.Action);
            Assert.IsNotNull(result.Team);
            Assert.AreEqual(123, result.Team.Id);
            Assert.IsNotNull(result.Organization);
            Assert.AreEqual("TestOrg", result.Organization.Login);
            Assert.IsNotNull(result.Sender);
            Assert.AreEqual("TestUser", result.Sender.Login);
            Assert.IsNotNull(result.Repository);
            Assert.AreEqual("owner/repo", result.Repository.FullName);
        }

        [TestMethod]
        public void MapGitHubEvent_MinimalGitHubEventDto_ReturnsGitHubEventWithNulls()
        {
            // Arrange
            var gitHubEventDto = new GitHubEventDto
            {
                Action = null,
                Team = null,
                Organization = null,
                Sender = null,
                Repository = null
            };

            // Act
            var result = ObjectMapper.MapGitHubEvent(gitHubEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(EventAction.Unknown, result.Action);
            Assert.IsNull(result.Team);
            Assert.IsNull(result.Organization);
            Assert.IsNull(result.Sender);
            Assert.IsNull(result.Repository);
        }
    }
}

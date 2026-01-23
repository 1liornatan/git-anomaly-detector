using GitAnomalyDetector.Dtos;
using GitAnomalyDetector.Models;
using GitAnomalyDetector.Utils;

namespace GitAnomalyDetector.Tests.Utils
{
    public class ObjectMapperTests
    {
        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(123, result.Id);
            Assert.Equal("TestTeam", result.Name);
        }

        [Fact]
        public void MapTeam_NullTeamDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapTeam(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(789, result.Id);
            Assert.Equal("owner/repo", result.FullName);
            Assert.Equal(new DateTime(2021, 12, 20, 13, 46, 40, DateTimeKind.Utc), result.PushedAt);
            Assert.Equal(new DateTime(2021, 8, 26, 15, 33, 20, DateTimeKind.Utc), result.CreatedAt);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(DateTime.MinValue, result.PushedAt);
            Assert.Equal(DateTime.MinValue, result.CreatedAt);
        }

        [Fact]
        public void MapRepository_NullRepositoryDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapRepository(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(111, result.Id);
            Assert.Equal("test-org", result.Login);
        }

        [Theory]
        [InlineData(null)]    // Null DTO
        [InlineData("")]      // Null login (empty string used as marker)
        public void MapOrganization_InvalidInput_ReturnsNull(string? marker)
        {
            // Arrange
            OrganizationDto? organizationDto = marker == null 
                ? null 
                : new OrganizationDto { Id = 222, Login = null };

            // Act
            var result = ObjectMapper.MapOrganization(organizationDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(333, result.Id);
            Assert.Equal("test-user", result.Login);
        }

        [Fact]
        public void MapUser_NullUserDto_ReturnsNull()
        {
            // Act
            var result = ObjectMapper.MapUser(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(444, result.Id);
            Assert.Equal(string.Empty, result.Login);
        }

        [Theory]
        [InlineData(1640000000, 2021, 12, 20, 13, 46, 40)]  // Standard timestamp
        [InlineData(0, 1970, 1, 1, 0, 0, 0)]                 // Unix epoch
        [InlineData(1800000000, 2027, 1, 15, 3, 20, 0)]      // Future timestamp
        public void UnixTimeStampToDateTime_ValidTimestamp_ReturnsCorrectDateTime(
            long timestamp, int year, int month, int day, int hour, int minute, int second)
        {
            // Act
            var result = ObjectMapper.UnixTimeStampToDateTime(timestamp);

            // Assert
            Assert.Equal(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc), result);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(EventAction.Created, result.Action);
            Assert.NotNull(result.Team);
            Assert.Equal(123, result.Team.Id);
            Assert.NotNull(result.Organization);
            Assert.Equal("TestOrg", result.Organization.Login);
            Assert.NotNull(result.Sender);
            Assert.Equal("TestUser", result.Sender.Login);
            Assert.NotNull(result.Repository);
            Assert.Equal("owner/repo", result.Repository.FullName);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(EventAction.Unknown, result.Action);
            Assert.Null(result.Team);
            Assert.Null(result.Organization);
            Assert.Null(result.Sender);
            Assert.Null(result.Repository);
        }
    }
}

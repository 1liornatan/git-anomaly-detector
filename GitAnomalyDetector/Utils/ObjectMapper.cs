using GitAnomalyDetector.Controllers;
using GitAnomalyDetector.Dtos;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Utils
{
    public static class ObjectMapper
    {
        public static GitHubEvent? MapGitHubEvent(GitHubEventDto gitHubEventDto)
        {
            var eventAction = EventTypesParser.ParseEventAction(gitHubEventDto.Action);

            return new GitHubEvent
            {
                Action = eventAction,
                Team = MapTeam(gitHubEventDto.Team),
                Organization = MapOrganization(gitHubEventDto.Organization),
                Sender = MapUser(gitHubEventDto.Sender),
                Repository = MapRepository(gitHubEventDto.Repository)
            };
        }

        public static Team? MapTeam(TeamDto? team)
        {
            if (team == null)
                return null;

            return new Team
            {
                Name = team.Name,
                Id = team.Id
            };
        }

        public static Repository? MapRepository(RepositoryDto? repository)
        {
            if (repository == null)
                return null;

            var pushedAt = repository.PushedAt.HasValue
                ? UnixTimeStampToDateTime(repository.PushedAt.Value)
                : DateTime.MinValue;

            var createdAt = repository.CreatedAt.HasValue
                ? UnixTimeStampToDateTime(repository.CreatedAt.Value)
                : DateTime.MinValue;

            return new Repository
            {
                Id = repository.Id,
                FullName = repository.FullName,
                PushedAt = pushedAt,
                CreatedAt = createdAt
            };
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
            return dateTime;
        }

        public static Organization? MapOrganization(OrganizationDto? organization)
        {
            if (organization?.Login == null)
                return null;
                
            return new Organization
            {
                Login = organization.Login,
                Id = organization.Id
            };
        }

        public static User? MapUser(UserDto? user)
        {
            if (user == null)
                return null;

            return new User
            {
                Login = user.Login ?? string.Empty,
                Id = user.Id
            };
        }
    }
}
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

        internal static Team? MapTeam(TeamDto? team)
        {
            if (team == null)
                return null;

            return new Team
            {
                Name = team.Name,
                Id = team.Id
            };
        }

        internal static Repository? MapRepository(RepositoryDto? repository)
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

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        internal static Organization? MapOrganization(OrganizationDto? organization)
        {
            if (organization?.Login == null)
                return null;
                
            return new Organization
            {
                Login = organization.Login,
                Id = organization.Id
            };
        }

        internal static User? MapUser(UserDto? user)
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
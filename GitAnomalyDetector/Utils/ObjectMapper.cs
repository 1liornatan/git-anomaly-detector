using GitAnomalyDetector.Dtos;
using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Utils
{
    public static class ObjectMapper
    {
        public static GitHubEvent? MapGitHubEvent(GitHubEventDto gitHubEventDto)
        {
            if (gitHubEventDto.Payload == null)
                return null;

            var eventType = Enum.TryParse<EventType>(gitHubEventDto.Event, true, out var parsedEventType)
                ? parsedEventType
                : EventType.Unknown;

            var eventAction = Enum.TryParse<EventAction>(gitHubEventDto.Payload.Action, true, out var parsedAction)
                ? parsedAction
                : EventAction.Unknown;

            return new GitHubEvent
            {
                Event = eventType,
                Payload = new Payload
                {
                    Action = eventAction,
                    Team = MapTeam(gitHubEventDto.Payload.Team),
                    Organization = MapOrganization(gitHubEventDto.Payload.Organization),
                    Sender = MapUser(gitHubEventDto.Payload.Sender),
                    Repository = MapRepository(gitHubEventDto.Payload.Repository)
                }
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

            return new Repository
            {
                Id = repository.Id,
                FullName = repository.FullName,
                PushedAt = pushedAt
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
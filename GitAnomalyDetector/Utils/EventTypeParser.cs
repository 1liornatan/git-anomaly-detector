using GitAnomalyDetector.Models;

namespace GitAnomalyDetector.Controllers
{
    internal static class EventTypesParser
    {
        public static EventType ParseEventType(string? eventType)
        {
            return Enum.TryParse<EventType>(eventType, true, out var parsedEventType)
                ? parsedEventType
                : EventType.Unknown;
        }

        internal static EventAction? ParseEventAction(string? action)
        {
            return Enum.TryParse<EventAction>(action, true, out var parsedAction)
                ? parsedAction
                : EventAction.Unknown;
        }
    }
}

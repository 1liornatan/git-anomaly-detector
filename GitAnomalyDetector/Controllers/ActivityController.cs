using GitAnomalyDetector.Dtos;
using GitAnomalyDetector.Services;
using GitAnomalyDetector.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GitAnomalyDetector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private const string GitHubEventTypeHeader = "X-GitHub-Event";
        private readonly IEventService _eventService;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(IEventService eventService, ILogger<ActivityController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GitHubEventDto gitHubEventDto)
        {
            var request = HttpContext.Request;
            _logger.LogInformation("Received GitHub event webhook");

            try
            {
                var gitHubEvent = ObjectMapper.MapGitHubEvent(gitHubEventDto);

                if (gitHubEvent == null)
                {
                    _logger.LogWarning("Failed to map GitHub event from DTO");
                    return BadRequest(Result.FailureResult("Failed to map event."));
                }

                if (request.Headers.TryGetValue(GitHubEventTypeHeader, out var eventType))
                {
                    gitHubEvent.Type = EventTypesParser.ParseEventType(eventType);
                    _logger.LogInformation($"Processing GitHub event type: {eventType}");
                }

                var result = await _eventService.HandleEventAsync(gitHubEvent);
                _logger.LogInformation("Event processing completed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GitHub event");
                return StatusCode(500, Result.FailureResult($"An error occurred: {ex.Message}"));
            }
        }
    }
}

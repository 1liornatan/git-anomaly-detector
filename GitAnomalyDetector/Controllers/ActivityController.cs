using GitAnomalyDetector.Common;
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
        private readonly IEventService _eventService;

        public ActivityController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GitHubEventDto gitHubEventDto)
        {
            // if(string.IsNullOrWhiteSpace(gitHubEventDto.Event))
            // {
            //     return BadRequest(Result.FailureResult("Event type is required."));
            // }
            var request = HttpContext.Request;

            try
            {
                var gitHubEvent = ObjectMapper.MapGitHubEvent(gitHubEventDto);

                if (gitHubEvent == null)
                {
                    return BadRequest(Result.FailureResult("Failed to map event."));
                }

                // set event type from header x-github-event
                if (request.Headers.TryGetValue("X-GitHub-Event", out var eventType))
                {
                    gitHubEvent.Type = EventTypesParser.ParseEventType(eventType);
                }

                var result = await _eventService.HandleEventAsync(gitHubEvent);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred: {ex.Message}"));
            }
        }
    }
}

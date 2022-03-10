using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Zkrd.Slack.WebApiDemo
{
    [ApiController]
    [Route("/helloworld/api/")]
    public class HelloWorldController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly SlackService _slackService;

        public HelloWorldController(ILogger<HelloWorldController> logger, SlackService slackService)
        {
            _logger = logger;
            _slackService = slackService;
        }

        [HttpGet]
        public IActionResult Get() => Ok("Hello World");

        [HttpPost]
        public async Task<IActionResult> Post(HelloWorldRequest request)
        {
            _logger.LogDebug(
                "Got HelloWorldRequest '{Message}' to channel '{Channel}'",
                request.Message,
                request.Channel);

            (bool success, string message) = await _slackService.PostMessage(request.Message, request.Channel);
            
            if (!success)
            {
                return NotFound(message);
            }

            return Ok();
        }
    }
}

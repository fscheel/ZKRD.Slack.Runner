using Microsoft.AspNetCore.Mvc;

namespace Zkrd.Slack.Runner
{
   [ApiController]
   [Route("/")]
   public class HelloWorldController : ControllerBase
   {
      [HttpGet("/")]
      public IActionResult Get()
      {
         return Ok("Hello World");
      }
   }
}

using Microsoft.AspNetCore.Mvc;

namespace Runner
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

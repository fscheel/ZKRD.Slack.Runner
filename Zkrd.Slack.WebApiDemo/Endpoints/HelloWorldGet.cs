using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Zkrd.Slack.WebApiDemo.Endpoints;

[HttpGet("/helloworld/api/")]
[AllowAnonymous]
public class HelloWorldGet: EndpointWithoutRequest
{
   public override async Task HandleAsync(CancellationToken ct)
   {
      await SendAsync("Hello World", cancellation: ct);
   }
}

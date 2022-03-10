using FastEndpoints;
using Microsoft.Extensions.Logging;
using Zkrd.Slack.WebApiDemo.ApiContracts;
using Zkrd.Slack.WebApiDemo.Services;

namespace Zkrd.Slack.WebApiDemo.Endpoints
{
   public class HelloWorldPost: Endpoint<HelloWorldRequest>
   {
      private readonly ISlackService _slackService;
      private readonly ILogger _logger;

      public HelloWorldPost(ILogger<HelloWorldPost> logger, ISlackService slackService)
      {
         _logger = logger;
         _slackService = slackService;
      }

      public override void Configure()
      {
         Verbs(Http.POST);
         Routes("/helloworld/api/");
         AllowAnonymous();
      }

      public override async Task HandleAsync(HelloWorldRequest req, CancellationToken ct)
      {
         _logger.LogDebug(
            "Got HelloWorldRequest '{Message}' to channel '{Channel}'",
            req.Message,
            req.Channel);

         (ApiResults success, string message) = await _slackService.PostMessage(req.Message, req.Channel);

         switch(success)
         {
            case ApiResults.NotFound:
               await SendAsync(message, 404, ct);
               break;
            case ApiResults.Error:
               await SendAsync(message, 400, ct);
               break;
            case ApiResults.Ok:
            default:
               await SendAsync(string.Empty, 200, ct);
               break;
         }
      }
   }
}

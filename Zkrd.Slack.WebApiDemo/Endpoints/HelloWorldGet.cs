using FastEndpoints;

namespace Zkrd.Slack.WebApiDemo;

public class HelloWorldGet: EndpointWithoutRequest
{
   public override void Configure()
   {
      Verbs(Http.GET);
      Routes("/helloworld/api/");
      AllowAnonymous();
   }

   public override async Task HandleAsync(CancellationToken ct)
   {
      await SendAsync("Hello World");
   }
}

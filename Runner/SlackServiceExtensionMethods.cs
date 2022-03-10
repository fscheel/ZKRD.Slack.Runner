using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;

namespace Runner
{
   public static class SlackServiceExtensionMethods
   {
      public static void AddSlackBackgroundService(this IServiceCollection services, IConfiguration configuration)
      {
         services.Configure<SlackOptions>(configuration.GetSection("SlackOptions"));
         services.AddHostedService<SlackService>();


         services.AddTransient(serviceProvider =>
         {
            IOptions<SlackOptions> config = serviceProvider.GetRequiredService<IOptions<SlackOptions>>();
            return new WebProxy(config.Value.Proxy.Host, config.Value.Proxy.Port);
         });
         services.AddScoped(serviceProvider => new SocketModeClient(() => new ClientWebSocket
         {
            Options = { Proxy = serviceProvider.GetRequiredService<WebProxy>() },
         }));
         services.AddScoped(serviceProvider =>
         {
            var client = new HttpClient(new HttpClientHandler
            {
               Proxy = serviceProvider.GetRequiredService<WebProxy>(),
            });
            client.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer",
                  serviceProvider.GetRequiredService<IOptions<SlackOptions>>().Value.Token);
            return client;
         });
         services.AddScoped(serviceProvider =>
         {
            HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();
            return new SlackWebApiClient(httpClient);
         });
      }
   }
}

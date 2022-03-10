using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;
using Zkrd.Slack.Core.BackgroundServices;

namespace Zkrd.Slack.Core
{
   public static class SlackServiceExtensionMethods
   {
      public static void AddSlackBackgroundService(this IServiceCollection services, IConfiguration configuration)
      {
         services.Configure<SlackOptions>(configuration.GetSection("SlackOptions"));
         services.AddHostedService<SlackBackgroundService>();
         services.AddHostedService<SlackMessageBackgroundDispatcher>();

         services.AddTransient(serviceProvider =>
         {
            IOptions<SlackOptions> config = serviceProvider.GetRequiredService<IOptions<SlackOptions>>();
            return config.Value.Proxy != null ? new WebProxy(config.Value.Proxy.Host!, config.Value.Proxy.Port) : new WebProxy();
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

         var slackReceiveChannel = System.Threading.Channels.Channel.CreateUnbounded<Envelope>();
         services.AddSingleton(slackReceiveChannel.Writer);
         services.AddSingleton(slackReceiveChannel.Reader);
         
         var slackSendChannel = System.Threading.Channels.Channel.CreateUnbounded<string>();
         services.AddSingleton(slackSendChannel.Writer);
         services.AddSingleton(slackSendChannel.Reader);
      }
   }
}

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
using Zkrd.Slack.Core.Services;

namespace Zkrd.Slack.Core
{
   public static class SlackServiceExtensionMethods
   {
      public static IServiceCollection AddSlackBackgroundService(this IServiceCollection services,
         IConfiguration configuration)
      {
         services.Configure<SlackCoreOptions>(configuration.GetSection("SlackCore"));
         services.Configure<ProxyOptions>(configuration.GetSection("Proxy"));
         services.AddHostedService<SlackBackgroundService>();
         services.AddHostedService<SlackMessageBackgroundDispatcher>();

         services.AddTransient(
            serviceProvider =>
            {
               var config = serviceProvider.GetService<IOptions<ProxyOptions>>();
               string? host = config?.Value.Host;
               int? port = config?.Value.Port;
               if ((host == null) != (port == null))
               {
                  throw new Exception("Proxy configuration must contain a host and port option if given.");
               }
               return host != null
                  ? new WebProxy(host, port!.Value)
                  : new WebProxy();
            });

         services.AddTransient(
            serviceProvider => new SocketModeClient(
               () => new ClientWebSocket
               {
                  Options = { Proxy = serviceProvider.GetRequiredService<WebProxy>() },
               }));

         services
            .AddHttpClient<SlackWebApiClient>((provider, client) =>
            {
               var config = provider.GetRequiredService<IOptions<SlackCoreOptions>>();
               client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", config.Value.BotToken);
            })
            .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
               new HttpClientHandler
               {
                  Proxy = serviceProvider.GetRequiredService<WebProxy>(),
               });
         services.AddTransient<ISlackApiClient>(serviceProvider =>
            serviceProvider.GetRequiredService<SlackWebApiClient>());
         services.AddTransient<ISlackReceiveService, SlackReceiveService>();
         services.AddTransient<ISlackMessageDispatchService, SlackMessageDispatchService>();

         var slackReceiveChannel = System.Threading.Channels.Channel.CreateUnbounded<Envelope>();
         services.AddSingleton(slackReceiveChannel.Writer);
         services.AddSingleton(slackReceiveChannel.Reader);
         return services;
      }
   }
}

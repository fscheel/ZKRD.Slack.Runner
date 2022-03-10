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
        public static IServiceCollection AddSlackBackgroundService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SlackOptions>(configuration.GetSection("SlackOptions"));
            services.AddHostedService<SlackBackgroundService>();
            services.AddHostedService<SlackMessageBackgroundDispatcher>();
            services.AddHttpClient(
                    nameof(HttpClientNames.ProxiedHttpClient),
                    (serviceProvider, client) =>
                    {
                        IOptions<SlackOptions> config = serviceProvider.GetRequiredService<IOptions<SlackOptions>>();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Value.BotToken);
                    })
                .ConfigurePrimaryHttpMessageHandler(
                    serviceProvider =>
                        new HttpClientHandler
                        {
                            Proxy = serviceProvider.GetRequiredService<WebProxy>(),
                        });

            services.AddTransient(
                serviceProvider =>
                {
                    IOptions<SlackOptions> config = serviceProvider.GetRequiredService<IOptions<SlackOptions>>();
                    return config.Value.Proxy != null ? new WebProxy(config.Value.Proxy.Host!, config.Value.Proxy.Port) : new WebProxy();
                });

            services.AddScoped(
                serviceProvider => new SocketModeClient(
                    () => new ClientWebSocket
                    {
                        Options = { Proxy = serviceProvider.GetRequiredService<WebProxy>() },
                    }));
            services.AddTransient(
                serviceProvider =>
                {
                    IHttpClientFactory client = serviceProvider.GetRequiredService<IHttpClientFactory>();
                    return new SlackWebApiClient(client.CreateClient(nameof(HttpClientNames.ProxiedHttpClient)));
                });
            services.AddTransient<ISlackApiClient>(serviceProvider => serviceProvider.GetRequiredService<SlackWebApiClient>());

            var slackReceiveChannel = System.Threading.Channels.Channel.CreateUnbounded<Envelope>();
            services.AddSingleton(slackReceiveChannel.Writer);
            services.AddSingleton(slackReceiveChannel.Reader);
            return services;
        }
    }
}

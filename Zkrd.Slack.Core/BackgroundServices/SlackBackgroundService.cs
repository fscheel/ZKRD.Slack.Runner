using System.Net.Http.Headers;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;

namespace Zkrd.Slack.Core.BackgroundServices
{
   public class SlackBackgroundService : BackgroundService
   {
      private readonly IServiceProvider _services;
      private readonly ILogger<SlackBackgroundService> _logger;
      private readonly ChannelWriter<Envelope> _receiveChannelWriter;
      private readonly IOptions<SlackOptions> _options;

      public SlackBackgroundService(
         ILogger<SlackBackgroundService> logger,
         IServiceProvider services,
         ChannelWriter<Envelope> receiveChannelWriter,
         IOptions<SlackOptions> options)
      {
         _logger = logger;
         _services = services;
         _receiveChannelWriter = receiveChannelWriter;
         _options = options;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         using IServiceScope scope = _services.CreateScope();
         try
         {
            SocketModeClient slackClient = scope.ServiceProvider.GetRequiredService<SocketModeClient>();
            SlackWebApiClient slackApiClient = scope.ServiceProvider.GetRequiredService<SlackWebApiClient>();
            slackApiClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.Value.AppToken);
            await slackClient.ConnectAsync(slackApiClient, stoppingToken);

            await foreach (Envelope envelope in slackClient.EnvelopeAsyncEnumerable(stoppingToken))
            {
               await slackClient.Acknowledge(envelope.EnvelopeId, stoppingToken);
               _logger.LogDebug("Received envelope {EnvelopeId} from slack", envelope.EnvelopeId);
               await _receiveChannelWriter.WriteAsync(envelope, stoppingToken);
            }
         }
         catch (OperationCanceledException)
         {
            _logger.LogDebug("Operation got cancelled");
         }
         catch (Exception e)
         {
            _logger.LogCritical(e, "exception when reading from slack");
            throw;
         }
      }
   }
}

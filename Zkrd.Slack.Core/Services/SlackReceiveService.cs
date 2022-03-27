using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;
using System.Net.Http.Headers;
using System.Threading.Channels;

namespace Zkrd.Slack.Core.Services;

public class SlackReceiveService : ISlackReceiveService
{
   private readonly ILogger<SlackReceiveService> _logger;
   private readonly ChannelWriter<Envelope> _receiveChannelWriter;
   private readonly SlackWebApiClient _slackApiClient;
   private readonly SocketModeClient _slackClient;

   public SlackReceiveService(ILogger<SlackReceiveService> logger, ChannelWriter<Envelope> receiveChannelWriter,
      IOptions<SlackCoreOptions> options, SlackWebApiClient slackApiClient, SocketModeClient slackClient)
   {
      _logger = logger;
      _receiveChannelWriter = receiveChannelWriter;
      _slackApiClient = slackApiClient;
      _slackClient = slackClient;
      _slackApiClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.AppToken);
   }

   public async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      try
      {
         await _slackClient.ConnectAsync(_slackApiClient, stoppingToken);

         await foreach (Envelope envelope in _slackClient.EnvelopeAsyncEnumerable(stoppingToken))
         {
            await _slackClient.Acknowledge(envelope.EnvelopeId, stoppingToken);
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

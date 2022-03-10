using Microsoft.Extensions.Logging;
using Slack.NetStandard.Socket;
using System.Threading.Channels;
using Zkrd.Slack.Core.BackgroundServices;
using Zkrd.Slack.Core.MessageHandlers;

namespace Zkrd.Slack.Core.Services;

public class SlackMessageDispatchService : ISlackMessageDispatchService
{
   private readonly ChannelReader<Envelope> _receiveChannelReader;
   private readonly ILogger<SlackMessageDispatchService> _logger;
   private readonly IEnumerable<ISyncSlackMessageHandler> _syncSlackMessageHandlers;
   private readonly IEnumerable<IAsyncSlackMessageHandler> _asyncSlackMessageHandlers;

   public SlackMessageDispatchService(ChannelReader<Envelope> receiveChannelReader,
      ILogger<SlackMessageDispatchService> logger,
      IEnumerable<ISyncSlackMessageHandler> syncSlackMessageHandlers,
      IEnumerable<IAsyncSlackMessageHandler> asyncSlackMessageHandlers)
   {
      _receiveChannelReader = receiveChannelReader;
      _logger = logger;
      _syncSlackMessageHandlers = syncSlackMessageHandlers;
      _asyncSlackMessageHandlers = asyncSlackMessageHandlers;
   }

   public async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      try
      {
         await foreach (Envelope envelope in _receiveChannelReader.ReadAllAsync(stoppingToken))
         {
            _logger.LogDebug("Received envelope {EnvelopeId} from channel", envelope.EnvelopeId);
            foreach (ISyncSlackMessageHandler messageHandler in _syncSlackMessageHandlers)
            {
               stoppingToken.ThrowIfCancellationRequested();
               messageHandler.HandleMessage(envelope, stoppingToken);
            }

            foreach (IAsyncSlackMessageHandler messageHandler in _asyncSlackMessageHandlers)
            {
               stoppingToken.ThrowIfCancellationRequested();
               await messageHandler.HandleMessageAsync(envelope, stoppingToken);
            }
         }
      }
      catch (OperationCanceledException)
      {
         _logger.LogDebug("Message Background Dispatcher cancelled");
      }
   }
}

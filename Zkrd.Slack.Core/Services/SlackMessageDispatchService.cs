using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slack.NetStandard.Socket;
using System.Threading.Channels;
using Zkrd.Slack.Core.MessageHandlers;

namespace Zkrd.Slack.Core.Services;

public class SlackMessageDispatchService : ISlackMessageDispatchService
{
   private readonly ChannelReader<Envelope> _receiveChannelReader;
   private readonly ILogger<SlackMessageDispatchService> _logger;

   private readonly IServiceProvider
      _serviceProvider; // Use a ServiceLocator pattern here to use fresh objects each time. That way, used HttpClients are able to follow DNS refreshes.

   public SlackMessageDispatchService(ChannelReader<Envelope> receiveChannelReader,
      ILogger<SlackMessageDispatchService> logger,
      IServiceProvider serviceProvider)
   {
      _receiveChannelReader = receiveChannelReader;
      _logger = logger;
      _serviceProvider = serviceProvider;
   }

   public async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      try
      {
         await foreach (Envelope envelope in _receiveChannelReader.ReadAllAsync(stoppingToken))
         {
            _logger.LogDebug("Received envelope {EnvelopeId} from channel", envelope.EnvelopeId);

            var syncMessageHandlers = _serviceProvider.GetRequiredService<IEnumerable<ISyncSlackMessageHandler>>();
            foreach (ISyncSlackMessageHandler messageHandler in syncMessageHandlers)
            {
               stoppingToken.ThrowIfCancellationRequested();
               messageHandler.HandleMessage(envelope, stoppingToken);
            }

            var asyncMessageHandlers = _serviceProvider.GetRequiredService<IEnumerable<IAsyncSlackMessageHandler>>();
            foreach (IAsyncSlackMessageHandler messageHandler in asyncMessageHandlers)
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

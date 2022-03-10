using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slack.NetStandard.Socket;
using Zkrd.Slack.Core.MessageHandlers;

namespace Zkrd.Slack.Core.BackgroundServices;

public class SlackMessageBackgroundDispatcher : BackgroundService
{
    private readonly ChannelReader<Envelope> _receiveChannelReader;
    private readonly ILogger<SlackMessageBackgroundDispatcher> _logger;
    private readonly IEnumerable<ISyncSlackMessageHandler> _syncSlackMessageHandlers;
    private readonly IEnumerable<IAsyncSlackMessageHandler> _asyncSlackMessageHandlers;

    public SlackMessageBackgroundDispatcher(
        ChannelReader<Envelope> receiveChannelReader,
        ILogger<SlackMessageBackgroundDispatcher> logger,
        IEnumerable<ISyncSlackMessageHandler> syncSlackMessageHandlers,
        IEnumerable<IAsyncSlackMessageHandler> asyncSlackMessageHandlers)
    {
        _receiveChannelReader = receiveChannelReader;
        _logger = logger;
        _syncSlackMessageHandlers = syncSlackMessageHandlers;
        _asyncSlackMessageHandlers = asyncSlackMessageHandlers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (Envelope envelope in _receiveChannelReader.ReadAllAsync(stoppingToken))
            {
                _logger.LogDebug("Received envelope {EnvelopeId} from channel", envelope.EnvelopeId);
                foreach (ISyncSlackMessageHandler messageHandler in _syncSlackMessageHandlers)
                {
                    messageHandler.HandleMessage(envelope);
                }

                foreach (IAsyncSlackMessageHandler messageHandler in _asyncSlackMessageHandlers)
                {
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
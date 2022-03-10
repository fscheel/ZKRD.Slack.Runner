using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slack.NetStandard.Socket;

namespace Zkrd.Slack.Core.BackgroundServices;

public class SlackMessageBackgroundDispatcher: BackgroundService
{
    private readonly ChannelReader<Envelope> _receiveChannelReader;
    private readonly ILogger<SlackMessageBackgroundDispatcher> _logger;

    public SlackMessageBackgroundDispatcher(ChannelReader<Envelope> receiveChannelReader, ILogger<SlackMessageBackgroundDispatcher> logger)
    {
        _receiveChannelReader = receiveChannelReader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (Envelope envelope in _receiveChannelReader.ReadAllAsync(stoppingToken))
            {
                _logger.LogDebug("Received envelope {EnvelopeId} from channel", envelope.EnvelopeId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Message Background Dispatcher cancelled");
        }
    }
}
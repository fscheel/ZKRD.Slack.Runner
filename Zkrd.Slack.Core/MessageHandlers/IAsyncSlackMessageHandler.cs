using Slack.NetStandard.Socket;

namespace Zkrd.Slack.Core.MessageHandlers;

public interface IAsyncSlackMessageHandler
{
    Task HandleMessageAsync(Envelope slackMessage, CancellationToken cancellationToken);
}
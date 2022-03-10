using Slack.NetStandard.Socket;

namespace Zkrd.Slack.Core.MessageHandlers;

public interface ISyncSlackMessageHandler
{
    void HandleMessage(Envelope slackMessage);
}
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using Zkrd.Slack.Core.MessageHandlers;

namespace Zkrd.Slack.FooBar;

public class Foobar : IAsyncSlackMessageHandler
{
    private readonly Regex _messageRegex = new(@"^<@\w+> foo$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly ISlackApiClient _apiClient;

    public Foobar(ISlackApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task HandleMessageAsync(Envelope slackMessage, CancellationToken stoppingToken = default)
    {
        if (slackMessage.Payload is EventCallback { Event: AppMention appMentionEvent })
        {
            if (_messageRegex.IsMatch(appMentionEvent.Text))
            {
                stoppingToken.ThrowIfCancellationRequested();
                await _apiClient.Chat.Post(
                    new PostMessageRequest
                    {
                        Channel = appMentionEvent.Channel,
                        Text = "Bar",
                    });
            }
        }
    }
}

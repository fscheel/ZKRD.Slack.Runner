using Microsoft.Extensions.Hosting;
using Zkrd.Slack.Core.Services;

namespace Zkrd.Slack.Core.BackgroundServices;

public class SlackMessageBackgroundDispatcher : BackgroundService
{
    private readonly ISlackMessageDispatchService _slackMessageDispatchService;

    public SlackMessageBackgroundDispatcher(ISlackMessageDispatchService slackMessageDispatchService)
    {
        _slackMessageDispatchService = slackMessageDispatchService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _slackMessageDispatchService.ExecuteAsync(stoppingToken);
    }
}

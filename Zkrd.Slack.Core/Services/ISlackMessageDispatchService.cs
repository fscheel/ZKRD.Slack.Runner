namespace Zkrd.Slack.Core.Services;

public interface ISlackMessageDispatchService
{
   Task ExecuteAsync(CancellationToken stoppingToken);
}

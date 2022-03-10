namespace Zkrd.Slack.Core.Services;

public interface ISlackReceiveService
{
   Task ExecuteAsync(CancellationToken stoppingToken);
}

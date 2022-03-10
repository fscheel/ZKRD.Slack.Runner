namespace Zkrd.Slack.WebApiDemo.Services;

public interface ISlackService
{
   Task<(ApiResults, string)> PostMessage(string message, string channelName);
}

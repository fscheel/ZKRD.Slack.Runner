namespace Zkrd.Slack.WebApiDemo;

public interface ISlackService
{
   Task<(ApiResults, string)> PostMessage(string message, string channelName);
}
namespace Zkrd.Slack.WebApiDemo.ApiContracts;

public class HelloWorldRequest
{
   public string Message { get; set; } = string.Empty;
   public string Channel { get; set; } = string.Empty;
}

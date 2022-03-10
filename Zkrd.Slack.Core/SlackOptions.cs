namespace Zkrd.Slack.Core
{
   public class SlackOptions
   {
      public string? AppToken { get; set; }
      public string? BotToken { get; set; }
      public Proxy? Proxy { get; set; }
   }

   public class Proxy
   {
      public string? Host { get; set; }
      public int Port { get; set; }
   }
}

namespace Runner
{
   public class SlackOptions
   {
      public string Token { get; set; }
      public Proxy Proxy { get; set; }
   }

   public class Proxy
   {
      public string Host { get; set; }
      public int Port { get; set; }
   }
}

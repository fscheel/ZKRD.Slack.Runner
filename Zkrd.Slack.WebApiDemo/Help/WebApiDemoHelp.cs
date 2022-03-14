using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Zkrd.Slack.Help.HelpComponents;

namespace Zkrd.Slack.WebApiDemo.Help;

public class WebApiDemoHelp : IModuleHelp
{
   public string Name => "WebApiDemo";
   public string ShortDescription => "Example service on how API endpoints in this bot.";

   public IList<IMessageBlock> DedicatedHelp => new List<IMessageBlock>
   {
      new Header("Help of the WebApiDemo module"),
      new Section(new MarkdownText("This is a simple example module. It was created to demonstrate how a programmer " +
                                   "can create an API endpoint for the bot. These endpoints allow other programs to " +
                                   "interact with the bot and retrieve data or send messages to slack.")),
      new Section(new MarkdownText(
         "This module adds 2 endpoints. One simply returns the string `Hello World` and the " +
         "other allows to send arbitrary text to a chosen channel.")),
      new Section("For more information in these, contact this bots administrator."),
   };
}

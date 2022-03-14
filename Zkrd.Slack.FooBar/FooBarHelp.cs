using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Zkrd.Slack.Help.HelpComponents;

namespace Zkrd.Slack.FooBar;

public class FooBarHelp: IModuleHelp
{
   public string Name => "FooBar";
   public string ShortDescription => "Example module that replies every `@<bot-name> foo` with a `bar`.";

   public IList<IMessageBlock> DedicatedHelp => new List<IMessageBlock>
   {
      new Header("Help of the FooBar module"),
      new Section(new MarkdownText("This is a simple example module. It was created as a demonstration, how a programmer " +
                                   "can react to an event from slack and send a message in return.")),
      new Section(new MarkdownText("To see this in action, type `@<bot-name> foo`.")),
   };
}

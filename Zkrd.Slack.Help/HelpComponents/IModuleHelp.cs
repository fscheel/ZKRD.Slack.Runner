using Slack.NetStandard.Messages.Blocks;

namespace Zkrd.Slack.Help.HelpComponents;

public interface IModuleHelp
{
   string Name { get; }
   string ShortDescription { get; }
   IList<IMessageBlock> DedicatedHelp { get; }
}

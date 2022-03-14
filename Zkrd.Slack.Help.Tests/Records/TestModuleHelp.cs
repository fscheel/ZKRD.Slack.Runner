using Slack.NetStandard.Messages.Blocks;
using System.Collections.Generic;
using Zkrd.Slack.Help.HelpComponents;

namespace Zkrd.Slack.Help.Tests.Records;

internal record TestModuleHelp(string Name,
   string ShortDescription,
   IList<IMessageBlock> DedicatedHelp) : IModuleHelp;

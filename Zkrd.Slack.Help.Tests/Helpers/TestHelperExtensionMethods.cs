using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using System;

namespace Zkrd.Slack.Help.Tests.Helpers;

public static class TestHelperExtensionMethods
{
   public static bool IsSectionWithMarkdownText(this IMessageBlock block, string textToContain)
   {
      return block.GetSectionMarkdownText() == textToContain;
   }

   public static string GetSectionMarkdownText(this IMessageBlock block)
   {
      var section = block as Section;
      var markdownText = section?.Text as MarkdownText;
      return markdownText?.Text ?? throw new InvalidOperationException("Not a Section or MarkdownText");
   }
}

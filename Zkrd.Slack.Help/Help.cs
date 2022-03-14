using Microsoft.Extensions.DependencyInjection;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System.Text;
using System.Text.RegularExpressions;
using Zkrd.Slack.Core.MessageHandlers;
using Zkrd.Slack.Help.HelpComponents;

namespace Zkrd.Slack.Help;

public class Help : IAsyncSlackMessageHandler
{
   private readonly Regex _messageRegex = new(@"^(?<mention><@\w+>) help(?: (?<module>\w+))?$",
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

   private readonly IServiceProvider _services;
   private readonly IEnumerable<IModuleHelp> _moduleHelpInfos;

   private const string IntroductoryText =
      "_beep-bopp_ Welcome to the help system. Let me introduce myself. I am your friendly neighbourhood bot.\n" +
      "I have near endless contingencies, but my current capabilities are limited to the loaded modules.\n" +
      "To learn more about any of them, use `{0} help <module>`.\n" +
      "\n" +
      "*Loaded modules:*\n" +
      "{1}";

   public Help(IServiceProvider services, IEnumerable<IModuleHelp> moduleHelpInfos)
   {
      _services = services;
      _moduleHelpInfos = moduleHelpInfos;
   }

   public async Task HandleMessageAsync(Envelope slackMessage, CancellationToken stoppingToken = default)
   {
      if (slackMessage.Payload is EventCallback { Event: AppMention appMentionEvent })
      {
         Match match = _messageRegex.Match(appMentionEvent.Text);
         if (match.Success)
         {
            stoppingToken.ThrowIfCancellationRequested();
            var apiClient = _services.GetRequiredService<ISlackApiClient>();
            IList<IMessageBlock> helpBlocks = GetHelpBlocks(match);

            await apiClient.Chat.Post(
               new PostMessageRequest
               {
                  Channel = appMentionEvent.Channel,
                  Blocks = helpBlocks,
               });
         }
      }
   }

   private IList<IMessageBlock> GetHelpBlocks(Match match) =>
      match.Groups["module"].Success == false ? GetTopLevelHelp(match) : GetModuleSpecificHelp(match);
   
   private IList<IMessageBlock> GetTopLevelHelp(Match match) =>
      new IMessageBlock[]
      {
         new Section(
            new MarkdownText(
               string.Format(IntroductoryText, match.Groups["mention"].Value, BuildModuleList())
            )
         ),
      };

   private string BuildModuleList()
   {
      if (!_moduleHelpInfos.Any())
      {
         return "none\n";
      }

      var sb = new StringBuilder();
      foreach (IModuleHelp helpInfo in _moduleHelpInfos.OrderBy(help => help.Name))
      {
         sb.AppendLine($"{helpInfo.Name} - {helpInfo.ShortDescription}");
      }

      return sb.ToString();
   }

   private IList<IMessageBlock> GetModuleSpecificHelp(Match match)
   {
      (string requestedModuleName, IModuleHelp? helpInfo) = FindRequestedModuleHelpInfo(match);
      return helpInfo == null ? BuildUnknownModuleText(requestedModuleName) : helpInfo.DedicatedHelp;
   }

   private (string requestedModuleName, IModuleHelp? helpInfo) FindRequestedModuleHelpInfo(Match match)
   {
      string requestedModuleName = match.Groups["module"].Value;
      IModuleHelp? helpInfo = _moduleHelpInfos.FirstOrDefault(help => help.Name == requestedModuleName);
      return (requestedModuleName, helpInfo);
   }

   private static IList<IMessageBlock> BuildUnknownModuleText(string module)
   {
      return new IMessageBlock[]
      {
         new Section(new MarkdownText($"I'm sorry but I can't give you any help about the module {module}")),
      };
   }
}

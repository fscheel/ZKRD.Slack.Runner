using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zkrd.Slack.Help.HelpComponents;
using Zkrd.Slack.Help.Tests.Helpers;

namespace Zkrd.Slack.Help.Tests.TestFixtures;

[TestFixture]
public class GeneralHelpTests
{
   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageWasNotAMention()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new BotMessage(),
         },
      };
      var sut = new Help(new List<IModuleHelp>(), serviceInstance);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MentionDidNotContainHelp()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "1234",
            },
         },
      };
      var sut = new Help(new List<IModuleHelp>(), serviceInstance);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MentionContainedAtButNotHelp()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new MessageCallbackEvent
            {
               Text = "<@154> 1234",
            },
         },
      };
      var sut = new Help(new List<IModuleHelp>(), serviceInstance);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task
      HandleMessageAsync_Should_PostMessageWithGeneralHelp_If_MessageWasAMentionWithHelp_And_NoModulesAreLoaded()
   {

      var serviceInstance = Substitute.For<ISlackApiClient>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> help",
            },
         },
      };
      var sut = new Help(new List<IModuleHelp>(), serviceInstance);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText(
            $"_beep-bopp_ Welcome to the help system. Let me introduce myself. I am your friendly neighbourhood bot.{Environment.NewLine}" +
            $"I have near endless contingencies, but my current capabilities are limited to the loaded modules.{Environment.NewLine}" +
            $"To learn more about any of them, use `<@154> help <module>`.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"*Loaded modules:*{Environment.NewLine}" +
            $"none{Environment.NewLine}")
      ));
   }
}

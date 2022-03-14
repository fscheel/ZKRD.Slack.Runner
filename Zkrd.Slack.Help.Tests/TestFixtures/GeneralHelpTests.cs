using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      services.AddService(typeof(ISlackApiClient), serviceInstance);
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new BotMessage(),
         },
      };
      var sut = new Help(services, new List<IModuleHelp>());

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MentionDidNotContainHelp()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      services.AddService(typeof(ISlackApiClient), serviceInstance);
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
      var sut = new Help(services, new List<IModuleHelp>());

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MentionContainedAtButNotHelp()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      services.AddService(typeof(ISlackApiClient), serviceInstance);
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
      var sut = new Help(services, new List<IModuleHelp>());

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task
      HandleMessageAsync_Should_PostMessageWithGeneralHelp_If_MessageWasAMentionWithHelp_And_NoModulesAreLoaded()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      services.AddService(typeof(ISlackApiClient), serviceInstance);
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
      var sut = new Help(services, new List<IModuleHelp>());

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText(
            "_beep-bopp_ Welcome to the help system. Let me introduce myself. I am your friendly neighbourhood bot.\n" +
            "I have near endless contingencies, but my current capabilities are limited to the loaded modules.\n" +
            "To learn more about any of them, use `<@154> help <module>`.\n" +
            "\n" +
            "*Loaded modules:*\n" +
            "none\n")
      ));
   }
}

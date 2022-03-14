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
using Zkrd.Slack.Help.Tests.Records;

namespace Zkrd.Slack.Help.Tests.TestFixtures;

[TestFixture]
public class ModuleHelpTests
{
   [Test]
   public async Task HandleMessageAsync_Should_ShowModuleHelp()
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
               Text = "<@154> help TestModule",
            },
         },
      };
      var moduleHelpInfos = new List<IModuleHelp>
      {
         new TestModuleHelp("TestModule", string.Empty, new List<IMessageBlock>
         {
            new Section(new MarkdownText("This is the long, long, long description of the module")),
         }),
      };
      var sut = new Help(services, moduleHelpInfos);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText("This is the long, long, long description of the module")
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ShowAllBlocksOfModuleHelp()
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
               Text = "<@154> help TestModule",
            },
         },
      };
      var moduleHelpInfos = new List<IModuleHelp>
      {
         new TestModuleHelp("TestModule", string.Empty, new List<IMessageBlock>
         {
            new Section(new MarkdownText("This is the long, long, long description of the module")),
            new Section(new MarkdownText("This is the second section for description of the module")),
            new Section(new MarkdownText("And a third")),
         }),
      };
      var sut = new Help(services, moduleHelpInfos);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.Count(block => block is Section) == 3
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ReturnErrorMessageIfModuleWasNotFound()
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
               Text = "<@154> help TestModule",
            },
         },
      };
      var sut = new Help(services, new List<IModuleHelp>());

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText("I'm sorry but I can't give you any help about the module TestModule")
      ));
   }
}

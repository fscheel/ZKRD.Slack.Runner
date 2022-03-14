using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System;
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
public class TopLevelHelpTests
{
   [Test]
   public async Task HandleMessageAsync_Should_ListModule_If_SingleModuleIsLoaded()
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
      var moduleHelpInfos = new List<IModuleHelp>
      {
         new TestModuleHelp("TestModule", "This is the description of TestModule", new List<IMessageBlock>()),
      };
      var sut = new Help(services, moduleHelpInfos);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText(
            "_beep-bopp_ Welcome to the help system. Let me introduce myself. I am your friendly neighbourhood bot.\n" +
            "I have near endless contingencies, but my current capabilities are limited to the loaded modules.\n" +
            "To learn more about any of them, use `<@154> help <module>`.\n" +
            "\n" +
            "*Loaded modules:*\n" +
            "TestModule - This is the description of TestModule\n")
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ListAllModules_If_MultipleModulesAreLoaded()
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
      var moduleHelpInfos = new List<IModuleHelp>
      {
         new TestModuleHelp("TestModule", "This is the description of TestModule", new List<IMessageBlock>()),
         new TestModuleHelp("TestModule2", "This is the description of TestModule2", new List<IMessageBlock>()),
         new TestModuleHelp("TestModule3", "This is the description of TestModule3", new List<IMessageBlock>()),
         new TestModuleHelp("TestModule4", "This is the description of TestModule4", new List<IMessageBlock>()),
      };
      var sut = new Help(services, moduleHelpInfos);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().IsSectionWithMarkdownText(
            "_beep-bopp_ Welcome to the help system. Let me introduce myself. I am your friendly neighbourhood bot.\n" +
            "I have near endless contingencies, but my current capabilities are limited to the loaded modules.\n" +
            "To learn more about any of them, use `<@154> help <module>`.\n" +
            "\n" +
            "*Loaded modules:*\n" +
            "TestModule - This is the description of TestModule\n" +
            "TestModule2 - This is the description of TestModule2\n" +
            "TestModule3 - This is the description of TestModule3\n" +
            "TestModule4 - This is the description of TestModule4\n")
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ListModulesInAlphabeticalOrder_If_MultipleModulesAreLoaded()
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
      var moduleHelpInfos = new List<IModuleHelp>
      {
         new TestModuleHelp("ATestModule", "This is the description of ATestModule", new List<IMessageBlock>()),
         new TestModuleHelp("CTestModule", "This is the description of CTestModule", new List<IMessageBlock>()),
         new TestModuleHelp("BTestModule", "This is the description of BTestModule", new List<IMessageBlock>()),
         new TestModuleHelp("FTestModule", "This is the description of FTestModule", new List<IMessageBlock>()),
      };
      var sut = new Help(services, moduleHelpInfos);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Blocks.First().GetSectionMarkdownText().EndsWith("ATestModule - This is the description of ATestModule\n" +
                                                            "BTestModule - This is the description of BTestModule\n" +
                                                            "CTestModule - This is the description of CTestModule\n" +
                                                            "FTestModule - This is the description of FTestModule\n")
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ThrowOperationAbortedException_If_CancellationWasRequested()
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
      var cts = new CancellationTokenSource();
      cts.Cancel();
      var sut = new Help(services, new List<IModuleHelp>());

      Func<Task> throwingAction = async () => await sut.HandleMessageAsync(input, cts.Token);

      await throwingAction.Should().ThrowAsync<OperationCanceledException>();
   }
}

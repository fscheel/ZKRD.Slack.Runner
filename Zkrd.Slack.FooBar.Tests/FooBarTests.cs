using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;

namespace Zkrd.Slack.FooBar.Tests;

[TestFixture]
public class FooBarTests
{
   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageWasNotAMention()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      serviceInstance.Chat.Post(default).ReturnsForAnyArgs(new PostMessageResponse());
      services.AddService(typeof(ISlackApiClient), serviceInstance);
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new BotMessage(),
         },
      };
      var sut = new Foobar(services);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageDidNotContainFooOnly()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      serviceInstance.Chat.Post(default).ReturnsForAnyArgs(new PostMessageResponse());
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
      var sut = new Foobar(services);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageSimpleTextMessageContainedMentionAndFooOnly()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      serviceInstance.Chat.Post(default).ReturnsForAnyArgs(new PostMessageResponse());
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
      var sut = new Foobar(services);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_PostMessageWithBar_If_MessageWasAMentionWithFooOnly()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      serviceInstance.Chat.Post(default).ReturnsForAnyArgs(new PostMessageResponse());
      services.AddService(typeof(ISlackApiClient), serviceInstance);
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> foo",
            },
         },
      };
      var sut = new Foobar(services);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Text == "Bar"
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_ThrowOperationAbortedException_If_CancellationWasRequested()
   {
      var services = new ServiceContainer();
      var serviceInstance = Substitute.For<ISlackApiClient>();
      serviceInstance.Chat.Post(default).ReturnsForAnyArgs(new PostMessageResponse());
      services.AddService(typeof(ISlackApiClient), serviceInstance);
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> foo",
            },
         },
      };
      var cts = new CancellationTokenSource();
      cts.Cancel();
      var sut = new Foobar(services);

      Func<Task> throwingAction = async () => await sut.HandleMessageAsync(input, cts.Token);

      await throwingAction.Should().ThrowAsync<OperationCanceledException>();
   }
}

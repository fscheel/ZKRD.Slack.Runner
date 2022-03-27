using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Slack.NetStandard.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Zkrd.Slack.Core.MessageHandlers;
using Zkrd.Slack.Core.Services;
using Zkrd.Slack.Core.Tests.Mocks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Zkrd.Slack.Core.Tests.Services;

[TestFixture]
public class SlackMessageDispatchServiceTests
{
   [Test]
   public async Task ExecuteAsync_Should_LogOperationAbortedException_If_Thrown()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> reader,
         MockLogger<SlackMessageDispatchService> logger, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> _) = Setup();
      reader.ReadAllAsync().ThrowsForAnyArgs<OperationCanceledException>();

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Message Background Dispatcher cancelled", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogNoReceivedMessage_If_NoneIsPresent()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> _,
         MockLogger<SlackMessageDispatchService> logger, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> _) = Setup();

      await sut.ExecuteAsync(CancellationToken.None);

      logger.ReceivedWithAnyArgs(0).Log(LogLevel.Debug, Arg.Any<object>(), Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_NotCallSyncHandler_If_NoMessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> _,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var syncHandler = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler);

      await sut.ExecuteAsync(CancellationToken.None);

      syncHandler.ReceivedWithAnyArgs(0).HandleMessage(Arg.Any<Envelope>());
   }

   [Test]
   public async Task ExecuteAsync_Should_NotCallAsyncHandler_If_NoMessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> _,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) = Setup();
      var asyncHandler = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler);

      await sut.ExecuteAsync(CancellationToken.None);

      await asyncHandler.ReceivedWithAnyArgs(0).HandleMessageAsync(Arg.Any<Envelope>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogReceivedMessage_If_MessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> logger, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var envelope = new Envelope
      {
         EnvelopeId = "EnvelopeId",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId from channel", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_CallSyncHandler_If_MessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var syncHandler = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler);
      var envelope = new Envelope
      {
         EnvelopeId = "EnvelopeId",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      syncHandler.Received(1).HandleMessage(envelope);
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAsyncHandler_If_MessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) = Setup();
      var asyncHandler = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler);
      var envelope = new Envelope
      {
         EnvelopeId = "EnvelopeId",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await asyncHandler.Received(1).HandleMessageAsync(envelope, Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAllSyncHandlers_If_MessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var syncHandler1 = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler1);
      var syncHandler2 = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler2);
      var envelope = new Envelope
      {
         EnvelopeId = "EnvelopeId",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      syncHandler1.Received(1).HandleMessage(envelope);
      syncHandler2.Received(1).HandleMessage(envelope);
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAllAsyncHandlers_If_MessageIsAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) = Setup();
      var asyncHandler1 = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler1);
      var asyncHandler2 = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler2);
      var envelope = new Envelope
      {
         EnvelopeId = "EnvelopeId",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await asyncHandler1.Received(1).HandleMessageAsync(envelope, Arg.Any<CancellationToken>());
      await asyncHandler2.Received(1).HandleMessageAsync(envelope, Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogAllReceivedMessages_If_MultipleMessagesAreAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> logger, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var envelope1 = new Envelope
      {
         EnvelopeId = "EnvelopeId1",
      };
      var envelope2 = new Envelope
      {
         EnvelopeId = "EnvelopeId2",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope1, envelope2 }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId1 from channel", Arg.Any<Exception>());
      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId2 from channel", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_CallSyncHandlerMultipleTimes_If_MultipleMessagesAreAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var syncHandler = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler);
      var envelope1 = new Envelope
      {
         EnvelopeId = "EnvelopeId1",
      };
      var envelope2 = new Envelope
      {
         EnvelopeId = "EnvelopeId2",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope1, envelope2 }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      syncHandler.Received(1).HandleMessage(envelope1);
      syncHandler.Received(1).HandleMessage(envelope2);
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAsyncHandlerMultipleTimes_If_MultipleMessagesAreAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) = Setup();
      var asyncHandler = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler);
      var envelope1 = new Envelope
      {
         EnvelopeId = "EnvelopeId1",
      };
      var envelope2 = new Envelope
      {
         EnvelopeId = "EnvelopeId2",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope1, envelope2 }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await asyncHandler.Received(1).HandleMessageAsync(envelope1, Arg.Any<CancellationToken>());
      await asyncHandler.Received(1).HandleMessageAsync(envelope2, Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAllSyncHandlers_If_MultipleMessagesAreAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
         List<IAsyncSlackMessageHandler> _) = Setup();
      var syncHandler1 = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler1);
      var syncHandler2 = Substitute.For<ISyncSlackMessageHandler>();
      syncSlackMessageHandlers.Add(syncHandler2);
      var envelope1 = new Envelope
      {
         EnvelopeId = "EnvelopeId1",
      };
      var envelope2 = new Envelope
      {
         EnvelopeId = "EnvelopeId2",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope1, envelope2 }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      syncHandler1.Received(1).HandleMessage(envelope1);
      syncHandler2.Received(1).HandleMessage(envelope1);
      syncHandler1.Received(1).HandleMessage(envelope2);
      syncHandler2.Received(1).HandleMessage(envelope2);
   }

   [Test]
   public async Task ExecuteAsync_Should_CallAllAsyncHandlers_If_MultipleMessagesAreAvailable()
   {
      (SlackMessageDispatchService sut, ChannelReader<Envelope> channelReader,
         MockLogger<SlackMessageDispatchService> _, List<ISyncSlackMessageHandler> _,
         List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) = Setup();
      var asyncHandler1 = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler1);
      var asyncHandler2 = Substitute.For<IAsyncSlackMessageHandler>();
      asyncSlackMessageHandlers.Add(asyncHandler2);
      var envelope1 = new Envelope
      {
         EnvelopeId = "EnvelopeId1",
      };
      var envelope2 = new Envelope
      {
         EnvelopeId = "EnvelopeId2",
      };
      channelReader.ReadAllAsync().ReturnsForAnyArgs(new List<Envelope> { envelope1, envelope2 }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await asyncHandler1.Received(1).HandleMessageAsync(envelope1, Arg.Any<CancellationToken>());
      await asyncHandler2.Received(1).HandleMessageAsync(envelope1, Arg.Any<CancellationToken>());
      await asyncHandler1.Received(1).HandleMessageAsync(envelope2, Arg.Any<CancellationToken>());
      await asyncHandler2.Received(1).HandleMessageAsync(envelope2, Arg.Any<CancellationToken>());
   }

   private static (SlackMessageDispatchService sut, ChannelReader<Envelope> receiveChannelReader,
      MockLogger<SlackMessageDispatchService> logger, List<ISyncSlackMessageHandler> syncSlackMessageHandlers,
      List<IAsyncSlackMessageHandler> asyncSlackMessageHandlers) Setup()
   {
      var receiveChannelReader = Substitute.For<ChannelReader<Envelope>>();
      var logger = Substitute.For<MockLogger<SlackMessageDispatchService>>();
      var syncSlackMessageHandlers = new List<ISyncSlackMessageHandler>();
      var asyncSlackMessageHandlers = new List<IAsyncSlackMessageHandler>();
      var serviceProvider = Substitute.For<IServiceProvider>();
      serviceProvider.GetService(typeof(IEnumerable<ISyncSlackMessageHandler>)).Returns(syncSlackMessageHandlers);
      serviceProvider.GetService(typeof(IEnumerable<IAsyncSlackMessageHandler>)).Returns(asyncSlackMessageHandlers);
      var sut = new SlackMessageDispatchService(receiveChannelReader, logger, serviceProvider);
      return (sut, receiveChannelReader, logger, syncSlackMessageHandlers, asyncSlackMessageHandlers);
   }
}

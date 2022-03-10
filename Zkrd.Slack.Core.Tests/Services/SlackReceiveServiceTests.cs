using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Zkrd.Slack.Core.Services;
using Zkrd.Slack.Core.Tests.Mocks;

namespace Zkrd.Slack.Core.Tests.Services;

[TestFixture]
public class SlackReceiveServiceTests
{
   [Test]
   public async Task ExecuteAsync_Should_ConnectToSlackApi()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient webApiClient, SocketModeClient socketModeClient) = Setup();

      await sut.ExecuteAsync(CancellationToken.None);

      await socketModeClient.Received(1).ConnectAsync(webApiClient, Arg.Any<CancellationToken>());
   }

   [Test]
   public void Constructor_Should_SetAuthorizationTokenOfUsedWebApiClient()
   {
      (SlackReceiveService _, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient webApiClient, SocketModeClient _) = Setup();

      // Constructor is run in Setup, so do nothing here

      webApiClient.Client.DefaultRequestHeaders.Authorization.Should()
         .BeEquivalentTo(new AuthenticationHeaderValue("Bearer", "this is the app token"));
   }

   [Test]
   public async Task ExecuteAsync_Should_Not_AcknowledgeAnything_If_NoMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();

      await sut.ExecuteAsync(CancellationToken.None);

      await socketModeClient.ReceivedWithAnyArgs(0).Acknowledge(default, default);
   }

   [Test]
   public async Task ExecuteAsync_Should_Not_WriteToChannel_If_NoMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> writer,
         IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient _) = Setup();

      await sut.ExecuteAsync(CancellationToken.None);

      await writer.ReceivedWithAnyArgs(0).WriteAsync(Arg.Any<Envelope>(), default);
   }

   [Test]
   public async Task ExecuteAsync_Should_Not_Log_If_NoMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope> _,
         IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient _) = Setup();

      await sut.ExecuteAsync(CancellationToken.None);

      logger.ReceivedWithAnyArgs(0).Log(LogLevel.Debug, Arg.Any<object>(), Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_AcknowledgeMessage_If_SingleMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         new() { EnvelopeId = "EnvelopeId" },
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await socketModeClient.Received(1).Acknowledge("EnvelopeId", Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_WriteEnvelopeToChannel_If_SingleMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> writer, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      var envelope = new Envelope { EnvelopeId = "EnvelopeId" };
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         envelope,
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await writer.Received(1).WriteAsync(envelope, Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogEnvelopeId_If_SingleMessageWasReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         new() { EnvelopeId = "EnvelopeId" },
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId from slack", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_AcknowledgeAllMessages_If_MultipleMessagesWereReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         new() { EnvelopeId = "EnvelopeId 1" },
         new() { EnvelopeId = "EnvelopeId 2" },
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await socketModeClient.Received(1).Acknowledge("EnvelopeId 1", Arg.Any<CancellationToken>());
      await socketModeClient.Received(1).Acknowledge("EnvelopeId 2", Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_WriteAllMessagesToChannel_If_MultipleMessagesWereReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> writer, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      var envelope1 = new Envelope { EnvelopeId = "EnvelopeId 1" };
      var envelope2 = new Envelope { EnvelopeId = "EnvelopeId 2" };
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         envelope1,
         envelope2,
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      await writer.Received(1).WriteAsync(envelope1, Arg.Any<CancellationToken>());
      await writer.Received(1).WriteAsync(envelope2, Arg.Any<CancellationToken>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogAllEnvelopeIds_If_MultipleMessagesWereReceived()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient _, SocketModeClient socketModeClient) = Setup();
      socketModeClient.EnvelopeAsyncEnumerable(CancellationToken.None).ReturnsForAnyArgs(new List<Envelope>
      {
         new() { EnvelopeId = "EnvelopeId 1" },
         new() { EnvelopeId = "EnvelopeId 2" },
      }.ToAsyncEnumerable());

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId 1 from slack", Arg.Any<Exception>());
      logger.Received(1).Log(LogLevel.Debug, "Received envelope EnvelopeId 2 from slack", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogCancellation_If_CancellationWasRequested()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient slackWebApiClient, SocketModeClient socketModeClient) = Setup();
      socketModeClient.ConnectAsync(slackWebApiClient, default).ThrowsForAnyArgs<OperationCanceledException>();

      await sut.ExecuteAsync(CancellationToken.None);

      logger.Received(1).Log(LogLevel.Debug, "Operation got cancelled", Arg.Any<Exception>());
   }

   [Test]
   public async Task ExecuteAsync_Should_LogException_If_ExceptionHappened()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient slackWebApiClient, SocketModeClient socketModeClient) = Setup();
      Exception exception = new();
      socketModeClient.ConnectAsync(slackWebApiClient, default).ThrowsForAnyArgs(exception);

      try
      {
         await sut.ExecuteAsync(CancellationToken.None);
      }
      catch (Exception)
      {
         // ignored
      }

      logger.Received(1).Log(LogLevel.Critical, "exception when reading from slack", exception);
   }

   [Test]
   public async Task ExecuteAsync_Should_RethrowException_If_ExceptionHappened()
   {
      (SlackReceiveService sut, MockLogger<SlackReceiveService> _, ChannelWriter<Envelope> _, IOptions<SlackOptions> _,
         SlackWebApiClient slackWebApiClient, SocketModeClient socketModeClient) = Setup();
      Exception exception = new();
      socketModeClient.ConnectAsync(slackWebApiClient, default).ThrowsForAnyArgs(exception);

      Func<Task> throwingAction = async () => await sut.ExecuteAsync(CancellationToken.None);

      await throwingAction.Should().ThrowAsync<Exception>().Where(exception1 => exception1 == exception);
   }

   private static (SlackReceiveService sut, MockLogger<SlackReceiveService> logger, ChannelWriter<Envelope>
      receiveChannelWriter, IOptions<SlackOptions> options, SlackWebApiClient slackWebApiClient, SocketModeClient
      socketModeClient) Setup()
   {
      var logger = Substitute.For<MockLogger<SlackReceiveService>>();
      var receiveChannelWriter = Substitute.For<ChannelWriter<Envelope>>();
      var options = Substitute.For<IOptions<SlackOptions>>();
      options.Value.Returns(new SlackOptions
      {
         AppToken = "this is the app token",
      });
      var slackWebApiClient = new SlackWebApiClient("disregard");
      var socketModeClient = Substitute.For<SocketModeClient>();
      var sut = new SlackReceiveService(logger, receiveChannelWriter, options,
         slackWebApiClient, socketModeClient);
      return (sut, logger, receiveChannelWriter, options, slackWebApiClient, socketModeClient);
   }
}

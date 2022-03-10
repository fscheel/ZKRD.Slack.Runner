using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.Objects;
using Slack.NetStandard.WebApi;
using Slack.NetStandard.WebApi.Chat;
using System;
using System.Threading.Tasks;
using Zkrd.Slack.WebApiDemo.Services;

namespace Zkrd.Slack.WebApiDemo.Tests.Services;

[TestFixture]
public class SlackServiceTests
{
   [Test]
   public async Task PostMessage_Should_ReturnNotFound_If_ChannelWasNotFound()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = Array.Empty<Channel>()
         });
      var sut = new SlackService(apiClient);

      (ApiResults success, string _) = await sut.PostMessage("foo", "bar");

      success.Should().Be(ApiResults.NotFound);
   }

   [Test]
   public async Task PostMessage_Should_ReturnCorrectMessage_If_ChannelWasNotFound()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = Array.Empty<Channel>()
         });
      var sut = new SlackService(apiClient);

      (ApiResults _, string errorMessage) = await sut.PostMessage("foo", "bar");

      errorMessage.Should().Be("Channel not found");
   }

   [Test]
   public async Task PostMessage_Should_ReturnError_If_PostingMessageWasNotOk()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      const string channelName = "bar";
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = new []{new Channel{Name = channelName}}
         });
      apiClient.Chat.Post(null).ReturnsForAnyArgs(
         new PostMessageResponse
         {
            OK = false,
            Error = "There was an error",
         }
      );

      var sut = new SlackService(apiClient);

      (ApiResults success, string _) = await sut.PostMessage("foo", channelName);

      success.Should().Be(ApiResults.Error);
   }

   [Test]
   public async Task PostMessage_Should_ReturnCorrectMessage_If_PostingMessageWasNotOk()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      const string channelName = "bar";
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = new []{new Channel{Name = channelName}}
         });
      apiClient.Chat.Post(null).ReturnsForAnyArgs(
         new PostMessageResponse
         {
            OK = false,
            Error = "There was an error",
         }
      );
      var sut = new SlackService(apiClient);

      (ApiResults _, string errorMessage) = await sut.PostMessage("foo", channelName);

      errorMessage.Should().Be("There was an error");
   }

   [Test]
   public async Task PostMessage_Should_ReturnOk_If_PostingMessageWasOk()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      const string channelName = "bar";
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = new []{new Channel{Name = channelName}}
         });
      apiClient.Chat.Post(null).ReturnsForAnyArgs(
         new PostMessageResponse
         {
            OK = true,
         }
      );

      var sut = new SlackService(apiClient);

      (ApiResults success, string _) = await sut.PostMessage("foo", channelName);

      success.Should().Be(ApiResults.Ok);
   }

   [Test]
   public async Task PostMessage_Should_ReturnNoMessage_If_PostingMessageWasOk()
   {
      var apiClient = Substitute.For<ISlackApiClient>();
      const string channelName = "bar";
      apiClient.Conversations.List(null).ReturnsForAnyArgs(
         new ChannelListResponse
         {
            Channels = new []{new Channel{Name = channelName}}
         });
      apiClient.Chat.Post(null).ReturnsForAnyArgs(
         new PostMessageResponse
         {
            OK = true,
         }
      );
      var sut = new SlackService(apiClient);

      (ApiResults _, string errorMessage) = await sut.PostMessage("foo", channelName);

      errorMessage.Should().BeEmpty();
   }
}

using FastEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Zkrd.Slack.WebApiDemo.ApiContracts;
using Zkrd.Slack.WebApiDemo.Endpoints;
using Zkrd.Slack.WebApiDemo.Services;
using Zkrd.Slack.WebApiDemo.Tests.Mocks;

namespace Zkrd.Slack.WebApiDemo.Tests.Endpoints;

[TestFixture]
public class HelloWorldPostTests
{
   private static (HelloWorldPost, MockLogger<HelloWorldPost>, ISlackService) Setup()
   {
      var mockLogger = Substitute.For<MockLogger<HelloWorldPost>>();
      var mockSlackService = Substitute.For<ISlackService>();
      var sut = Factory.Create<HelloWorldPost>(mockLogger, mockSlackService);
      return (sut, mockLogger, mockSlackService);
   }

   [Test]
   public async Task Post_Should_ReturnNotFound_If_SlackServiceReturnsNotFound()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.NotFound, "Something not found"));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.HttpContext.Response.StatusCode.Should().Be(404);
   }

   [Test]
   public async Task Post_Should_ReturnWhatIsNotFound_If_SlackServiceReturnsNotFound()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.NotFound, "Something not found"));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.Response.Should().Be("Something not found");
   }

   [Test]
   public async Task Post_Should_ReturnBadRequest_If_SlackServiceReturnsError()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Error, "Something went wrong"));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.HttpContext.Response.StatusCode.Should().Be(400);
   }

   [Test]
   public async Task Post_Should_ReturnErrorMessage_If_SlackServiceReturnsError()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Error, "Something went wrong"));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.Response.Should().Be("Something went wrong");
   }

   [Test]
   public async Task Post_Should_ReturnOk_If_SlackServiceReturnsOk()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Ok, string.Empty));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.HttpContext.Response.StatusCode.Should().Be(200);
   }

   [Test]
   public async Task Post_Should_ReturnNoMessage_If_SlackServiceReturnsOk()
   {
      (HelloWorldPost sut, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Ok, string.Empty));

      await sut.HandleAsync(new HelloWorldRequest(), default);

      sut.Response.Should().BeOfType<string>().Which.Should().BeEmpty();
   }

   [Test]
   public async Task Post_Should_LogCall()
   {
      (HelloWorldPost sut, MockLogger<HelloWorldPost> logger, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.Ok, string.Empty));


      await sut.HandleAsync(new HelloWorldRequest
      {
         Channel = "meow",
         Message = "Hey there",
      }, default);

      logger.Received(1).Log(
         LogLevel.Debug,
         "Got HelloWorldRequest 'Hey there' to channel 'meow'");
   }
}

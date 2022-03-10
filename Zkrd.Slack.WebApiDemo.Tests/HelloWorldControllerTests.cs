using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Zkrd.Slack.WebApiDemo.Tests;

[TestFixture]
public class HelloWorldControllerTests
{
   [Test]
   public void Get_Should_ReturnHelloWorld()
   {
      var logger = Substitute.For<ILogger<HelloWorldController>>();
      var slackService = Substitute.For<ISlackService>();
      var sut = new HelloWorldController(logger, slackService);

      IActionResult result = sut.Get();

      result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("Hello World");
   }

   [Test]
   public async Task Post_Should_ReturnNotFound_If_SlackServiceReturnsNotFound()
   {
      var logger = Substitute.For<ILogger<HelloWorldController>>();
      var slackService = Substitute.For<ISlackService>();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.NotFound, "Something not found"));
      var sut = new HelloWorldController(logger, slackService);

      IActionResult result = await sut.Post(new HelloWorldRequest());

      result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Something not found");
   }

   [Test]
   public async Task Post_Should_ReturnBadRequest_If_SlackServiceReturnsError()
   {
      var logger = Substitute.For<ILogger<HelloWorldController>>();
      var slackService = Substitute.For<ISlackService>();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Error, "Something went wrong"));
      var sut = new HelloWorldController(logger, slackService);

      IActionResult result = await sut.Post(new HelloWorldRequest());

      result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Something went wrong");
   }

   [Test]
   public async Task Post_Should_ReturnOk_If_SlackServiceReturnsOk()
   {
      var logger = Substitute.For<ILogger<HelloWorldController>>();
      var slackService = Substitute.For<ISlackService>();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Ok, string.Empty));
      var sut = new HelloWorldController(logger, slackService);

      IActionResult result = await sut.Post(new HelloWorldRequest());

      result.Should().BeOfType<OkResult>();
   }

   [Test]
   public async Task Post_Should_LogCall()
   {
      var logger = Substitute.For<MockLogger<HelloWorldController>>();
      var slackService = Substitute.For<ISlackService>();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Ok, string.Empty));
      var sut = new HelloWorldController(logger, slackService);

      await sut.Post(new HelloWorldRequest
      {
         Channel = "meow",
         Message = "Hey there",
      });

      logger.Received(1).Log(
         LogLevel.Debug,
         "Got HelloWorldRequest 'Hey there' to channel 'meow'");
   }
}

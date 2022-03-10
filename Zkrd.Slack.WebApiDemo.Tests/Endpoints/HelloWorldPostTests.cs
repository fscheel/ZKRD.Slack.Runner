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
   private static (HttpClient, MockLogger<HelloWorldPost>, ISlackService) Setup()
   {
      var mockLogger = Substitute.For<MockLogger<HelloWorldPost>>();
      var mockSlackService = Substitute.For<ISlackService>();
      var factory = new WebApplicationFactory<Program>();
      HttpClient mockClient = factory
         .WithWebHostBuilder(builder =>
         {
            builder.ConfigureTestServices(collection =>
            {
               collection.AddSingleton<ILogger<HelloWorldPost>>(mockLogger);
               collection.AddSingleton(mockSlackService);
            });
         })
         .CreateClient();
      return (mockClient, mockLogger, mockSlackService);
   }

   [Test]
   public async Task Post_Should_ReturnNotFound_If_SlackServiceReturnsNotFound()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.NotFound, "Something not found"));

      (HttpResponseMessage? response, _) =
         await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      response.Should().HaveStatusCode(HttpStatusCode.NotFound);
   }

   [Test]
   public async Task Post_Should_ReturnWhatIsNotFound_If_SlackServiceReturnsNotFound()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.NotFound, "Something not found"));

      (_, string? result) = await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      result.Should().Be("Something not found");
   }

   [Test]
   public async Task Post_Should_ReturnBadRequest_If_SlackServiceReturnsError()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Error, "Something went wrong"));

      (HttpResponseMessage? response, _) =
         await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
   }

   [Test]
   public async Task Post_Should_ReturnErrorMessage_If_SlackServiceReturnsError()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.NotFound, "Something went wrong"));

      (_, string? result) = await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      result.Should().Be("Something went wrong");
   }

   [Test]
   public async Task Post_Should_ReturnBOk_If_SlackServiceReturnsOk()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty).ReturnsForAnyArgs((ApiResults.Ok, string.Empty));

      (HttpResponseMessage? response, _) =
         await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      response.Should().HaveStatusCode(HttpStatusCode.OK);
   }

   [Test]
   public async Task Post_Should_ReturnNoMessage_If_SlackServiceReturnsOk()
   {
      (HttpClient client, _, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.Ok, string.Empty));

      (_, string? result) = await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest());

      result.Should().BeEmpty();
   }

   [Test]
   public async Task Post_Should_LogCall()
   {
      (HttpClient client, MockLogger<HelloWorldPost> logger, ISlackService slackService) = Setup();
      slackService.PostMessage(string.Empty, string.Empty)
         .ReturnsForAnyArgs((ApiResults.Ok, string.Empty));


      await client.POSTAsync<HelloWorldPost, HelloWorldRequest, string>(new HelloWorldRequest
      {
         Channel = "meow",
         Message = "Hey there",
      });

      logger.Received(1).Log(
         LogLevel.Debug,
         "Got HelloWorldRequest 'Hey there' to channel 'meow'");
   }
}

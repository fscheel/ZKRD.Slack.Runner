using FastEndpoints;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using Zkrd.Slack.WebApiDemo.Endpoints;

namespace Zkrd.Slack.WebApiDemo.Tests.Endpoints;

[TestFixture]
public class HelloWorldGetTests
{
   [Test]
   public async Task HelloWorldGet_Should_Return_HelloWorld()
   {
      var sut = Factory.Create<HelloWorldGet>();

      await sut.HandleAsync(default);

      sut.Response.Should().Be("Hello World");
   }

   [Test]
   public async Task HelloWorldGet_Should_Return_Ok()
   {
      var sut = Factory.Create<HelloWorldGet>();

      await sut.HandleAsync(default);

      sut.HttpContext.Response.StatusCode.Should().Be(200);
   }
}

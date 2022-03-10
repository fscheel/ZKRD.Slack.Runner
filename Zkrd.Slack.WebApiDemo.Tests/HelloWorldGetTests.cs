using FastEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zkrd.Slack.WebApiDemo.Tests;

[TestFixture]
public class HelloWorldGetTests
{
   private static HttpClient GetClient()
   {
      var factory = new WebApplicationFactory<Program>();
      return factory.CreateClient();
   }

   [Test]
   public async Task HelloWorldGet_Should_Return_HelloWorld()
   {
      HttpClient client = GetClient();
      (_, string? result) = await client.GETAsync<HelloWorldGet, string>();
      result.Should().Be("Hello World");
   }

   [Test]
   public async Task HelloWorldGet_Should_Return_Ok()
   {
      HttpClient client = GetClient();
      (HttpResponseMessage? response, _) = await client.GETAsync<HelloWorldGet, string>();
      response.Should().HaveStatusCode(HttpStatusCode.OK);
   }
}

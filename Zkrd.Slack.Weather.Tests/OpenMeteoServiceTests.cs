using FluentAssertions;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System.Threading;
using System.Threading.Tasks;
using VerifyNUnit;
using Zkrd.Slack.Weather.Responses;
using Zkrd.Slack.Weather.Services;

namespace Zkrd.Slack.Weather.Tests;

[TestFixture]
public class OpenMeteoServiceTests
{
   private const string UrlToTest = "https://api.open-meteo.com/v1/forecast/?latitude=48.400&longitude=9.983&daily=weathercode,temperature_2m_max,temperature_2m_min&timezone=Europe%2FBerlin";

   [Test]
   public async Task Should_ReturnErrorCode_If_NoResponseWasReturnedByApi()
   {
      var handler = new MockHttpMessageHandler();
      handler.When(UrlToTest)
         .Respond("application/json", "null");
      var client = handler.ToHttpClient();
      var sut = new OpenMeteoService(client);

      (OperationSuccess success, OpenMeteoResponse _) = await sut.GetWeatherInUlmAsync(CancellationToken.None);

      success.Should().Be(OperationSuccess.Error);
   }

   [Test]
   public async Task Should_ReturnSuccessCode_If_ValidResponseWasReturnedByApi()
   {
      var handler = new MockHttpMessageHandler();
      handler.When(UrlToTest)
         .Respond("application/json", "{\"latitude\":48.4,\"longitude\":9.98,\"generationtime_ms\":0.5830526351928711,\"utc_offset_seconds\":7200,\"timezone\":\"Europe/Berlin\",\"timezone_abbreviation\":\"CEST\",\"elevation\":479.0,\"daily_units\":{\"time\":\"iso8601\",\"weathercode\":\"wmo code\",\"temperature_2m_max\":\"\u00B0C\",\"temperature_2m_min\":\"\u00B0C\"},\"daily\":{\"time\":[\"2022-09-14\",\"2022-09-15\",\"2022-09-16\",\"2022-09-17\",\"2022-09-18\",\"2022-09-19\",\"2022-09-20\"],\"weathercode\":[80.0,80.0,61.0,61.0,61.0,61.0,61.0],\"temperature_2m_max\":[24.4,18.3,15.8,11.5,14.4,14.1,13.5],\"temperature_2m_min\":[16.7,11.1,10.5,7.3,6.9,8.3,6.2]}}");
      var client = handler.ToHttpClient();
      var sut = new OpenMeteoService(client);

      (OperationSuccess success, OpenMeteoResponse _) = await sut.GetWeatherInUlmAsync(CancellationToken.None);

      success.Should().Be(OperationSuccess.Success);
   }

   [Test]
   public async Task Should_ReturnResponse_If_ValidResponseWasReturnedByApi()
   {
      var handler = new MockHttpMessageHandler();
      handler.When(UrlToTest)
         .Respond("application/json", "{\"latitude\":48.4,\"longitude\":9.98,\"generationtime_ms\":0.5830526351928711,\"utc_offset_seconds\":7200,\"timezone\":\"Europe/Berlin\",\"timezone_abbreviation\":\"CEST\",\"elevation\":479.0,\"daily_units\":{\"time\":\"iso8601\",\"weathercode\":\"wmo code\",\"temperature_2m_max\":\"\u00B0C\",\"temperature_2m_min\":\"\u00B0C\"},\"daily\":{\"time\":[\"2022-09-14\",\"2022-09-15\",\"2022-09-16\",\"2022-09-17\",\"2022-09-18\",\"2022-09-19\",\"2022-09-20\"],\"weathercode\":[80.0,80.0,61.0,61.0,61.0,61.0,61.0],\"temperature_2m_max\":[24.4,18.3,15.8,11.5,14.4,14.1,13.5],\"temperature_2m_min\":[16.7,11.1,10.5,7.3,6.9,8.3,6.2]}}");
      var client = handler.ToHttpClient();
      var sut = new OpenMeteoService(client);

      (OperationSuccess _, OpenMeteoResponse response) = await sut.GetWeatherInUlmAsync(CancellationToken.None);

      await Verifier.Verify(response);
   }
}

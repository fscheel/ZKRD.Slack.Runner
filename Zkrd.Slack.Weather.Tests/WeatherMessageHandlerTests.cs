using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zkrd.Slack.Weather.Converter;
using Zkrd.Slack.Weather.Responses;
using Zkrd.Slack.Weather.Services;

namespace Zkrd.Slack.Weather.Tests;

[TestFixture]
public class WeatherMessageHandlerTests
{
   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageWasNotAMention()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      var converter = Substitute.For<IWeatherResponseConverter>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new BotMessage(),
         },
      };
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_MessageDidNotContainWeatherOnly()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      var converter = Substitute.For<IWeatherResponseConverter>();
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
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_Not_PostMessage_If_TextMessageContainedMentionButNotWeather()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      var converter = Substitute.For<IWeatherResponseConverter>();
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
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.DidNotReceiveWithAnyArgs().Post(default);
   }

   [Test]
   public async Task HandleMessageAsync_Should_PostMessageWithWeather_If_MessageWasAMentionWithWeatherOnly()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      var converter = Substitute.For<IWeatherResponseConverter>();
      converter.Convert(null!).ReturnsForAnyArgs(new[]
      {
         new WeatherData
         {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Condition = "slight rain showers",
            MinimumTemperature = 5,
            MaximumTemperature = 10,
         }
      });
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> weather",
            },
         },
      };
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Text == "The weather in Ulm tomorrow is slight rain showers with min 5°C and max 10°C"
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_PostErrorMessage_If_ResponseWasNotSuccess()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      openMeteoService.GetWeatherInUlmAsync(CancellationToken.None).ReturnsForAnyArgs((OperationSuccess.Error, new OpenMeteoResponse()));
      var converter = Substitute.For<IWeatherResponseConverter>();
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> weather",
            },
         },
      };
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      await sut.HandleMessageAsync(input, CancellationToken.None);

      await serviceInstance.Chat.Received(1).Post(Arg.Is<PostMessageRequest>(p =>
         p.Text == "This didn't work out so well. Please try again later."
      ));
   }

   [Test]
   public async Task HandleMessageAsync_Should_Throw_If_ResponseDidNotContainTomorrowsWeather()
   {
      var serviceInstance = Substitute.For<ISlackApiClient>();
      var openMeteoService = Substitute.For<IOpenMeteoService>();
      var converter = Substitute.For<IWeatherResponseConverter>();
      converter.Convert(null!).ReturnsForAnyArgs(new[]
      {
         new WeatherData
         {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Condition = "slight rain showers",
            MinimumTemperature = 5,
            MaximumTemperature = 10,
         }
      });
      var input = new Envelope
      {
         Payload = new EventCallback
         {
            Event = new AppMention
            {
               Text = "<@154> weather",
            },
         },
      };
      var sut = new WeatherMessageHandler(serviceInstance, openMeteoService, converter);

      Func<Task> throwingAction = async () => await sut.HandleMessageAsync(input, CancellationToken.None);

      await throwingAction.Should().ThrowAsync<InvalidOperationException>();
   }
}

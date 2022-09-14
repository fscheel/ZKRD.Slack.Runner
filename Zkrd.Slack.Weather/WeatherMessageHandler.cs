using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi.Chat;
using System.Text.RegularExpressions;
using Zkrd.Slack.Core.MessageHandlers;
using Zkrd.Slack.Weather.Converter;
using Zkrd.Slack.Weather.Responses;
using Zkrd.Slack.Weather.Services;

namespace Zkrd.Slack.Weather;

public class WeatherMessageHandler : IAsyncSlackMessageHandler
{
   private readonly Regex _messageRegex = new(@"^<@\w+> weather$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
   private readonly ISlackApiClient _slackApiClient;
   private readonly IOpenMeteoService _weatherService;
   private readonly IWeatherResponseConverter _converter;

   public WeatherMessageHandler(ISlackApiClient slackApiClient, IOpenMeteoService weatherService,
      IWeatherResponseConverter converter)
   {
      _slackApiClient = slackApiClient;
      _weatherService = weatherService;
      _converter = converter;
   }

   public async Task HandleMessageAsync(Envelope slackMessage, CancellationToken cancellationToken = default)
   {
      if (slackMessage.Payload is EventCallback { Event: AppMention appMentionEvent })
      {
         if (_messageRegex.IsMatch(appMentionEvent.Text))
         {
            (OperationSuccess success, OpenMeteoResponse weatherResponse) =
               await _weatherService.GetWeatherInUlmAsync(cancellationToken);
            if (success == OperationSuccess.Error)
            {
               await _slackApiClient.Chat.Post(
                  new PostMessageRequest
                  {
                     Channel = appMentionEvent.Channel,
                     Text = "This didn't work out so well. Please try again later.",
                  });
               return;
            }

            IEnumerable<WeatherData> convertedResponse = _converter.Convert(weatherResponse);
            WeatherData tomorrowWeather = convertedResponse.First(data => data.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
            cancellationToken.ThrowIfCancellationRequested();
            await _slackApiClient.Chat.Post(
               new PostMessageRequest
               {
                  Channel = appMentionEvent.Channel,
                  Text =
                     $"The weather in Ulm tomorrow is {tomorrowWeather.Condition} with min {tomorrowWeather.MinimumTemperature}°C and max {tomorrowWeather.MaximumTemperature}°C",
               });
         }
      }
   }
}

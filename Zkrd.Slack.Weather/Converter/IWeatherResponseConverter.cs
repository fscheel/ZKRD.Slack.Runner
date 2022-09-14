using Zkrd.Slack.Weather.Responses;

namespace Zkrd.Slack.Weather.Converter;

public interface IWeatherResponseConverter
{
   IEnumerable<WeatherData> Convert(OpenMeteoResponse response);
}
using Zkrd.Slack.Weather.Responses;

namespace Zkrd.Slack.Weather.Converter;

public class WeatherResponseConverter : IWeatherResponseConverter
{
   public IEnumerable<WeatherData> Convert(OpenMeteoResponse response)
   {
      IList<WeatherData> returnValue = new List<WeatherData>();
      for (var i = 0; i < response.Daily.Time.Length; i++)
      {
         returnValue.Add(new WeatherData
         {
            Date = DateOnly.FromDateTime(response.Daily.Time[i]),
            Condition = ConvertWeatherCode(response.Daily.Weathercode[i]),
            MaximumTemperature = response.Daily.Temperature2mMax[i],
            MinimumTemperature = response.Daily.Temperature2mMin[i],
         });
      }

      return returnValue;
   }

   private static string ConvertWeatherCode(decimal code)
   {
      return code switch
      {
         0 => "clear",
         1 => "mainly clear",
         2 => "partly cloudy",
         3 => "overcast",
         45 => "fog",
         48 => "depositing rime fog",
         51 => "light drizzle",
         53 => "moderate drizzle",
         55 => "dense drizzle",
         56 => "light freezing drizzle",
         57 => "dense freezing drizzle",
         61 => "slight rain",
         63 => "moderate rain",
         65 => "heavy rain",
         66 => "light freezing rain",
         67 => "heavy freezing rain",
         71 => "slight snowfall",
         73 => "moderate snowfall",
         75 => "heavy snowfall",
         77 => "snow grains",
         80 => "slight rain showers",
         81 => "moderate rain showers",
         82 => "violent rain showers",
         85 => "slight snow showers",
         86 => "heavy snow showers",
         95 => "thunderstorm",
         96 => "thunderstorm with slight hail",
         99 => "thunderstorm with heavy hail",
         _ => $"Unknown weather condition {code}",
      };
   }
}

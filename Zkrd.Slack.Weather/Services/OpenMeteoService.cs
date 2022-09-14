using System.Net.Http.Json;
using Zkrd.Slack.Weather.Responses;

namespace Zkrd.Slack.Weather.Services;

public class OpenMeteoService : IOpenMeteoService
{
   private readonly HttpClient _client;

   public OpenMeteoService(HttpClient client)
   {
      _client = client;
      _client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
   }

   public async Task<(OperationSuccess, OpenMeteoResponse)> GetWeatherInUlmAsync(CancellationToken token = default)
   {
      var response = await _client.GetFromJsonAsync<OpenMeteoResponse>(
         "forecast/?latitude=48.400&longitude=9.983&daily=weathercode,temperature_2m_max,temperature_2m_min&timezone=Europe%2FBerlin",
         token);
      return response == null ? (OperationSuccess.Error, new OpenMeteoResponse()) : (OperationSuccess.Success, response);
   }
}

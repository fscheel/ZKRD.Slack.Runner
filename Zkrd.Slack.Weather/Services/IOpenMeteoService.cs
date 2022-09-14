using System.Net.Http.Headers;
using Zkrd.Slack.Weather.Responses;

namespace Zkrd.Slack.Weather.Services;

public interface IOpenMeteoService
{
   Task<(OperationSuccess, OpenMeteoResponse)> GetWeatherInUlmAsync(CancellationToken token);
}

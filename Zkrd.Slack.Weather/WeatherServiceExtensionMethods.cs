using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Zkrd.Slack.Core.MessageHandlers;
using Zkrd.Slack.Weather.Converter;
using Zkrd.Slack.Weather.Services;

namespace Zkrd.Slack.Weather;

public static class WeatherServiceExtensionMethods
{
   public static IServiceCollection AddWeatherService(this IServiceCollection serviceCollection)
   {
      serviceCollection.AddSingleton<IWeatherResponseConverter, WeatherResponseConverter>();
      serviceCollection.AddHttpClient<IOpenMeteoService, OpenMeteoService>()
         .ConfigurePrimaryHttpMessageHandler(provider =>
         {
            var proxy = provider.GetRequiredService<WebProxy>();
            return new HttpClientHandler
            {
               Proxy = proxy,
            };
         });
      serviceCollection.AddTransient<IAsyncSlackMessageHandler, WeatherMessageHandler>();
      return serviceCollection;
   }
}

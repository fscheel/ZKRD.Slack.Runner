using Microsoft.Extensions.DependencyInjection;

namespace Zkrd.Slack.WebApiDemo;

public static class ServiceExtensionMethods
{
   public static IServiceCollection AddHelloWorldController(this IServiceCollection services)
   {
      services.AddTransient<SlackService>();
      return services;
   }
}

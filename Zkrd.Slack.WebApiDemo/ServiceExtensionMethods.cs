using Microsoft.Extensions.DependencyInjection;
using Zkrd.Slack.WebApiDemo.Services;

namespace Zkrd.Slack.WebApiDemo;

public static class ServiceExtensionMethods
{
   public static IServiceCollection AddHelloWorldController(this IServiceCollection services)
   {
      services.AddTransient<ISlackService, SlackService>();
      return services;
   }
}

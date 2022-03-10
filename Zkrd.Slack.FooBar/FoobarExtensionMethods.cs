using Zkrd.Slack.Core.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Zkrd.Slack.FooBar;

public static class FoobarExtensionMethods
{
    public static IServiceCollection AddSlackFoobar(this IServiceCollection services)
    {
        services.AddSingleton<IAsyncSlackMessageHandler, Foobar>();
        return services;
    }
}
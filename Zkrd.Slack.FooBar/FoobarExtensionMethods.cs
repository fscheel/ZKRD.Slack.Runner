using Zkrd.Slack.Core.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Zkrd.Slack.Help.HelpComponents;

namespace Zkrd.Slack.FooBar;

public static class FoobarExtensionMethods
{
    public static IServiceCollection AddSlackFoobar(this IServiceCollection services)
    {
        services.AddTransient<IAsyncSlackMessageHandler, Foobar>()
            .AddSingleton<IModuleHelp, FooBarHelp>();
        return services;
    }
}

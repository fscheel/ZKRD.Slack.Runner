using Microsoft.Extensions.DependencyInjection;
using Zkrd.Slack.Core.MessageHandlers;

namespace Zkrd.Slack.Help;

public static class HelpExtensionMethods
{
    public static IServiceCollection AddSlackBotHelp(this IServiceCollection services)
    {
        services.AddTransient<IAsyncSlackMessageHandler, Help>();
        return services;
    }
}

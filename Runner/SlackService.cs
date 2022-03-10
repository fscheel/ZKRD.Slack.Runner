using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runner
{
   public class SlackService : BackgroundService
   {
      private readonly IServiceProvider _services;
      private readonly ILogger<SlackService> _logger;

      public SlackService(ILogger<SlackService> logger, IServiceProvider services)
      {
         _logger = logger;
         _services = services;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         using IServiceScope scope = _services.CreateScope();
         try
         {
            SocketModeClient slackClient = scope.ServiceProvider.GetRequiredService<SocketModeClient>();
            SlackWebApiClient slackApiClient = scope.ServiceProvider.GetRequiredService<SlackWebApiClient>();
            await slackClient.ConnectAsync(slackApiClient, stoppingToken);
            await foreach (Envelope envelope in slackClient.EnvelopeAsyncEnumerable(stoppingToken))
            {
               _logger.LogInformation("Processing {EnvelopeId}", envelope.EnvelopeId);
            }
         }
         catch (OperationCanceledException)
         {
            _logger.LogDebug("Operation got cancelled");
         }
         catch (Exception e)
         {
            _logger.LogCritical(e, "exception when reading from slack");
            throw;
         }
      }
   }
}

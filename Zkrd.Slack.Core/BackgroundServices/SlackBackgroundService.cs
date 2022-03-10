using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;

namespace Zkrd.Slack.Core.BackgroundServices
{
   public class SlackBackgroundService : BackgroundService
   {
      private readonly IServiceProvider _services;
      private readonly ILogger<SlackBackgroundService> _logger;
      private readonly ChannelWriter<Envelope> _receiveChannelWriter;
      private readonly ChannelReader<string> _sendChannelReader;

      public SlackBackgroundService(
         ILogger<SlackBackgroundService> logger,
         IServiceProvider services,
         ChannelWriter<Envelope> receiveChannelWriter,
         ChannelReader<string> sendChannelReader)
      {
         _logger = logger;
         _services = services;
         _receiveChannelWriter = receiveChannelWriter;
         _sendChannelReader = sendChannelReader;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         using IServiceScope scope = _services.CreateScope();
         try
         {
            SocketModeClient slackClient = scope.ServiceProvider.GetRequiredService<SocketModeClient>();
            SlackWebApiClient slackApiClient = scope.ServiceProvider.GetRequiredService<SlackWebApiClient>();
            await slackClient.ConnectAsync(slackApiClient, stoppingToken);
            var receiveTask = Task.Run(
               async () =>
               {
                  await foreach (Envelope envelope in slackClient.EnvelopeAsyncEnumerable(stoppingToken))
                  {
                     _logger.LogDebug("Received envelope {EnvelopeId} from slack", envelope.EnvelopeId);
                     await _receiveChannelWriter.WriteAsync(envelope, stoppingToken);
                  }
               },
               stoppingToken);
            var sendTask = Task.Run(
               async () =>
               {
                  await foreach (string message in _sendChannelReader.ReadAllAsync(stoppingToken))
                  {
                     await slackClient.Send(message, stoppingToken);
                  }
               },
               stoppingToken);
            Task.WaitAll(new[] { receiveTask, sendTask }, stoppingToken);
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

using Microsoft.Extensions.Hosting;
using Zkrd.Slack.Core.Services;

namespace Zkrd.Slack.Core.BackgroundServices
{
   public class SlackBackgroundService : BackgroundService
   {
      private readonly ISlackReceiveService _slackReceiveService;

      public SlackBackgroundService(ISlackReceiveService slackReceiveService)
      {
         _slackReceiveService = slackReceiveService;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         await _slackReceiveService.ExecuteAsync(stoppingToken);
      }
   }
}

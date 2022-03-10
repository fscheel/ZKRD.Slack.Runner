using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Objects;
using Slack.NetStandard.WebApi;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.Conversations;

namespace Zkrd.Slack.WebApiDemo;

public class SlackService : ISlackService
{
   private readonly ISlackApiClient _slackApi;

   public SlackService(ISlackApiClient slackApi)
   {
      _slackApi = slackApi;
   }

   public async Task<(ApiResults, string)> PostMessage(string message, string channelName)
   {
      ChannelListResponse channels = await _slackApi.Conversations.List(new ConversationListRequest());
      Channel? channel = channels.Channels.FirstOrDefault(channel1 => channel1.Name == channelName);
      if (channel == null)
      {
         {
            return (ApiResults.NotFound, "Channel not found");
         }
      }

      PostMessageResponse response = await _slackApi.Chat.Post(
         new PostMessageRequest
         {
            Channel = channel.ID,
            Blocks = new List<IMessageBlock>
            {
               new Header("Hello from API"),
               new Section(message),
            }
         });
      return response.OK ? (ApiResults.Ok, string.Empty) : (ApiResults.Error, response.Error);
   }
}

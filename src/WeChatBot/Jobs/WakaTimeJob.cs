using DryIoc;
using Quartz;
using System.Threading.Tasks;
using WeChatBot.MessageQueues;
using WeChatBot.Models.Messages;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class WakaTimeJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var messageQueue = container.Resolve<MessageQueue>();
        var wakaTimeService = container.Resolve<WakaTimeService>();
        var message = await wakaTimeService.GetMessageAsync();

        messageQueue.Enqueue(new TextMessage("Waka Time", message));
    }
}
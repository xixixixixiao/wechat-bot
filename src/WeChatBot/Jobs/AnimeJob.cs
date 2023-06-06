using DryIoc;
using Quartz;
using System.Threading.Tasks;
using WeChatBot.MessageQueues;
using WeChatBot.Models.Messages;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class AnimeJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var messageQueue = container.Resolve<MessageQueue>();
        var animeService = container.Resolve<AnimeService>();
        var message = await animeService.GetMessageAsync();

        messageQueue.Enqueue(new TextMessage("Anime", message));
    }
}
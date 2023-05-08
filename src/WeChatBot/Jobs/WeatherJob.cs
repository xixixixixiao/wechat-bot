using DryIoc;
using Quartz;
using System.Threading.Tasks;
using WeChatBot.MessageQueues;
using WeChatBot.Models.Messages;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class WeatherJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var weatherService = container.Resolve<WeatherService>();
        var messageQueue = container.Resolve<MessageQueue>();
        var message = await weatherService.GetMessageAsync();

        messageQueue.Enqueue(new TextMessage("Weather", message));
    }
}
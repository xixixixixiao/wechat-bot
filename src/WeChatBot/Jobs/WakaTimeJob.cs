using DryIoc;
using Quartz;
using Serilog;
using System.Threading.Tasks;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class WakaTimeJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var logger = container.Resolve<ILogger>();
        var automateService = container.Resolve<AutomateService>();
        var wakaTimeService = container.Resolve<WakaTimeService>();
      
        var message = await wakaTimeService.GetMessageAsync();
        var result = await automateService.SendTextMessageAsync(message);

        if (result)
        {
            logger.Information("Waka Time sent successfully.");
        }
        else
        {
            logger.Error("Waka Time sent failed.");
        }
    }
}
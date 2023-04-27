using DryIoc;
using Quartz;
using Serilog;
using System;
using System.Threading.Tasks;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class DailyNewsJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var logger = container.Resolve<ILogger>();
        var automateService = container.Resolve<AutomateService>();
        var dailyNewsService = container.Resolve<DailyNewsService>();

        var message = await dailyNewsService.GetMessageAsync();

        if (string.IsNullOrEmpty(message))
        {
            // Today's Daily News at now has not been spidered yet.
            // It will be executed again after a delay of 5 minutes.
            var trigger = TriggerBuilder.Create().StartAt(DateTimeOffset.Now.AddMinutes(5)).Build();
            await context.Scheduler.RescheduleJob(new TriggerKey(context.Trigger.Key.Name), trigger);
            logger.Warning("Today's Daily News at now has not been spidered yet. Retry after 5 minutes.");
            return;
        }

        var result = await automateService.SendTextMessageAsync(message);
        if (result)
        {
            logger.Information("Daily News sent successfully.");
        }
        else
        {
            logger.Error("Daily News sent failed.");
        }
    }
}
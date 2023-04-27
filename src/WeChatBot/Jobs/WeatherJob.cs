using DryIoc;
using Quartz;
using Serilog;
using System.Threading.Tasks;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class WeatherJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var logger = container.Resolve<ILogger>();
        var automateService = container.Resolve<AutomateService>();
        var weatherService = container.Resolve<WeatherService>();
        
        var message = await weatherService.GetMessageAsync();
        var result = await automateService.SendTextMessageAsync(message);

        if (result)
        {
            logger.Information("Weather sent successfully.");
        }
        else
        {
            logger.Error("Weather sent failed.");
        }
    }
}
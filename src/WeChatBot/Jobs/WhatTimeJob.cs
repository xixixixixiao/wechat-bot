using DryIoc;
using Quartz;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WeChatBot.Services;

namespace WeChatBot.Jobs;

public class WhatTimeJob : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var container = context.JobDetail.JobDataMap["Container"] as IContainer;
        var logger = container.Resolve<ILogger>();
        var automateService = container.Resolve<AutomateService>();

        var file = $"{DateTime.Now:hh}.gif";
        var path = await CacheGif(file);
        var result = await automateService.SendFileMessageAsync(path);

        if (result)
        {
            logger.Information($"send file: {path} successfully.");
        }
        else
        {
            logger.Error($"send file: {path} failed.");
        }
    }

    public static async Task<string> CacheGif(string file)
    {
        var temp = Path.GetTempPath();
        var path = Path.Combine(temp, file);

        if (File.Exists(path))
        {
            return path;
        }

        var uri = $"Assets/WhatTime/{file}";
        var info = Application.GetResourceStream(new Uri(uri, UriKind.Relative))?.Stream ?? Stream.Null;
        await using var fileStream = File.Create(path);
        info.Seek(0, SeekOrigin.Begin);
        await info.CopyToAsync(fileStream);

        return path;
    }
}
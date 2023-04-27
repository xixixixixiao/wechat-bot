using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WeChatBot.Jobs;
using WeChatBot.Services;

namespace WeChatBot.ViewModels;

[INotifyPropertyChanged]
public partial class MainViewModel
{
    private readonly ILogger _logger;
    private readonly IContainer _container;

    private IScheduler _scheduler;

    public MainViewModel(ILogger logger, IContainer container)
    {
        _logger = logger;
        _container = container;
    }

    /// <summary>
    /// Whether to enable sending Waka Time message.
    /// </summary>
    [ObservableProperty]
    private bool _enableWakaTime = true;

    /// <summary>
    /// The moment when sending the Waka Time message.
    /// Every day at 09:30:00.
    /// </summary>
    [ObservableProperty]
    private string _wakaTimeMoment = "0 30 9 * * ? *";

    /// <summary>
    /// Whether to enable sending weather forecast messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableWeather = true;

    /// <summary>
    /// The moment when sending the weather forecast message.
    /// Every day at 08:30:00 and 17:30:00.
    /// </summary>
    [ObservableProperty]
    private string _weatherMoment = "0 30 8,17 * * ? *";

    /// <summary>
    /// Whether to enable sending Daily News messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableDailyNews = true;

    /// <summary>
    /// The moment when sending the Daily News message.
    /// Every day at 09:10:00.
    /// </summary>
    [ObservableProperty]
    private string _dailyNewsMoment = "0 10 9 * * ? *";

    /// <summary>
    /// Whether to enable sending What Time is it.
    /// </summary>
    [ObservableProperty]
    private bool _enableWhatTime = true;

    /// <summary>
    /// The moment when sending What Time is it message.
    /// Every hour: 00/01/02/03/...
    /// </summary>
    [ObservableProperty]
    private string _whatTimeMoment = "0 0 0/1 * * ? *";

    /// <summary>
    /// Attach the Wechat.exe.
    /// </summary>
    [RelayCommand]
    public void AttachWechat()
    {
        var result = _container.Resolve<AutomateService>().AttachWechat();
        if (result)
        {
            NotifyInfo("Wechat has been attached successfully.");
        }
        else
        {
            NotifyWarning("Unable to attach WeChat.");
        }
    }

    /// <summary>
    /// Start the task.
    /// </summary>
    [RelayCommand]
    public async Task StartTaskAsync()
    {
        if (_scheduler?.IsStarted ?? false)
        {
            NotifyInfo("The task has been started.");
            return;
        }

        _scheduler = await new StdSchedulerFactory().GetScheduler();
        var dict = new Dictionary<string, object>
        {
            ["Container"] = _container,
        };
        var data = new JobDataMap((IDictionary<string, object>)dict);

        if (EnableDailyNews)
        {
            var dailyNewsJobDetail = JobBuilder.Create<DailyNewsJob>().SetJobData(data).Build();
            var dailyNewsTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(DailyNewsMoment).Build();

            await _scheduler.ScheduleJob(dailyNewsJobDetail, dailyNewsTrigger);
        }

        if (EnableWakaTime)
        {
            var wakaTimeJobDetail = JobBuilder.Create<WakaTimeJob>().SetJobData(data).Build();
            var wakaTimeTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WakaTimeMoment).Build();

            await _scheduler.ScheduleJob(wakaTimeJobDetail, wakaTimeTrigger);
        }

        if (EnableWeather)
        {
            var weatherJobDetail = JobBuilder.Create<WeatherJob>().SetJobData(data).Build();
            var weatherTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WeatherMoment).Build();

            await _scheduler.ScheduleJob(weatherJobDetail, weatherTrigger);
        }

        if (EnableWhatTime)
        {
            var whatTimeJobDetail = JobBuilder.Create<WhatTimeJob>().SetJobData(data).Build();
            var whatTimeTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WhatTimeMoment).Build();

            await _scheduler.ScheduleJob(whatTimeJobDetail, whatTimeTrigger);
        }

        await _scheduler.Start();
        NotifyInfo("Started successfully.");
    }

    /// <summary>
    /// Stop the task.
    /// </summary>
    [RelayCommand]
    public async Task StopTaskAsync()
    {
        if (_scheduler.IsShutdown)
        {
            NotifyInfo("The task has already been stopped.");
            return;
        }

        await _scheduler.Shutdown();
        NotifyInfo("The task has been stopped.");
    }

    /// <summary>
    /// Send a test message.
    /// </summary>
    [RelayCommand]
    public async Task SendTestMessageAsync()
    {
        await _container
            .Resolve<AutomateService>()
            .SendTextMessageAsync("Wechat has been attached successfully.");
    }

    /// <summary>
    /// Send all messages.
    /// </summary>
    [RelayCommand]
    public async Task SendAllMessagesAsync()
    {
        await SendWakaTimeMessageAsync();
        await SendWeatherMessageAsync();
        await SendDailyNewsMessageAsync();
        await SendWhatTimeMessageAsync();
    }

    /// <summary>
    /// Send the leaderboard message of Waka Time.
    /// </summary>
    [RelayCommand]
    public async Task SendWakaTimeMessageAsync()
    {
        var message = await _container.Resolve<WakaTimeService>().GetMessageAsync();
        var result = await _container.Resolve<AutomateService>().SendTextMessageAsync(message);

        if (result)
        {
            _logger.Information("Send the leaderboard message of Waka Time successfully.");
        }
        else
        {
            _logger.Error("Send the leaderboard message of Waka Time failed.");
        }
    }

    /// <summary>
    /// Send the weather message.
    /// </summary>
    [RelayCommand]
    public async Task SendWeatherMessageAsync()
    {
        var message = await _container.Resolve<WeatherService>().GetMessageAsync();
        var result = await _container.Resolve<AutomateService>().SendTextMessageAsync(message);

        if (result)
        {
            _logger.Information("Send the weather message successfully.");
        }
        else
        {
            _logger.Error("Send the weather message failed.");
        }
    }

    /// <summary>
    /// Send the Daily News message.
    /// </summary>
    [RelayCommand]
    public async Task SendDailyNewsMessageAsync()
    {
        var message = await _container.Resolve<DailyNewsService>().GetMessageAsync();

        if (string.IsNullOrEmpty(message))
        {
            NotifyInfo("There is no Daily News right now.");
            return;
        }

        var result = await _container.Resolve<AutomateService>().SendTextMessageAsync(message);
        if (result)
        {
            _logger.Information("Send the Daily News message successfully.");
        }
        else
        {
            _logger.Error("Send the Daily News message failed.");
        }
    }

    /// <summary>
    /// Send What Time is is message.
    /// </summary>
    [RelayCommand]
    public async Task SendWhatTimeMessageAsync()
    {
        var automateService = _container.Resolve<AutomateService>();

        var file = $"{DateTime.Now:hh}.gif";
        var path = await WhatTimeJob.CacheGif(file);
        var result = await automateService.SendFileMessageAsync(path);
        if (result)
        {
            _logger.Information("Send What Time is is message successfully.");
        }
        else
        {
            _logger.Error("Send What Time is is message failed.");
        }
    }

    #region Notify

    private static void NotifyInfo(string message)
    {
        MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void NotifyWarning(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    #endregion
}
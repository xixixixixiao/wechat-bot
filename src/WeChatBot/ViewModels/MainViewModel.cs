using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WeChatBot.Jobs;
using WeChatBot.MessageQueues;
using WeChatBot.Models.Messages;
using WeChatBot.Services;

namespace WeChatBot.ViewModels;

[INotifyPropertyChanged]
public partial class MainViewModel
{
    private readonly IContainer _container;
    private readonly IConfiguration _configuration;
    private readonly MessageQueue _queue;

    private IScheduler _scheduler;

    public MainViewModel(IContainer container, IConfiguration configuration, MessageQueue queue)
    {
        _container = container;
        _configuration = configuration;
        _queue = queue;
    }

    /// <summary>
    /// Whether to enable sending Waka Time message.
    /// </summary>
    [ObservableProperty]
    private bool _enableWakaTime;

    /// <summary>
    /// The cron when sending the Waka Time message.
    /// Every day at 10:30:00.
    /// </summary>
    [ObservableProperty]
    private string _wakaTimeCron;

    /// <summary>
    /// Whether to enable sending weather forecast messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableWeather;

    /// <summary>
    /// The cron when sending the weather forecast message.
    /// Every day at 08:30:00 and 17:30:00.
    /// </summary>
    [ObservableProperty]
    private string _weatherCron;

    /// <summary>
    /// Whether to enable sending Daily News messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableDailyNews;

    /// <summary>
    /// The cron when sending the Daily News message.
    /// Every day at 09:30:00.
    /// </summary>
    [ObservableProperty]
    private string _dailyNewsCron;

    /// <summary>
    /// Whether to enable sending What Time is it.
    /// </summary>
    [ObservableProperty]
    private bool _enableWhatTime;

    /// <summary>
    /// The cron when sending What Time is it message.
    /// Every hour, between 08:00 AM and 11:59 PM.
    /// Only on Monday, Tuesday, Wednesday, Thursday, and Friday.
    /// (Make Er Miao Happy)
    /// </summary>
    [ObservableProperty]
    private string _whatTimeCron;

    /// <summary>
    /// Initialize view model.
    /// </summary>
    [RelayCommand]
    public void Initialize()
    {
        _queue.Start();

        var cronSection = _configuration.GetSection("Cron");
        WakaTimeCron = cronSection.GetValue("WakaTime", "0 30 10 * * ? *");
        WeatherCron = cronSection.GetValue("Weather", "0 30 8,17 * * ? *");
        DailyNewsCron = cronSection.GetValue("DailyNews", "0 30 9 * * ? *");
        WhatTimeCron = cronSection.GetValue("WhatTime", "0 0 8-23 ? * MON,TUE,WED,THU,FRI *");

        EnableWakaTime = _configuration.GetValue("EnableWakaTime", true);
        EnableWeather = _configuration.GetValue("EnableWeather", true);
        EnableDailyNews = _configuration.GetValue("EnableDailyNews", true);
        EnableWhatTime = _configuration.GetValue("EnableWhatTime", true);
    }

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
            var dailyNewsTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(DailyNewsCron).Build();

            await _scheduler.ScheduleJob(dailyNewsJobDetail, dailyNewsTrigger);
        }

        if (EnableWakaTime)
        {
            var wakaTimeJobDetail = JobBuilder.Create<WakaTimeJob>().SetJobData(data).Build();
            var wakaTimeTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WakaTimeCron).Build();

            await _scheduler.ScheduleJob(wakaTimeJobDetail, wakaTimeTrigger);
        }

        if (EnableWeather)
        {
            var weatherJobDetail = JobBuilder.Create<WeatherJob>().SetJobData(data).Build();
            var weatherTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WeatherCron).Build();

            await _scheduler.ScheduleJob(weatherJobDetail, weatherTrigger);
        }

        if (EnableWhatTime)
        {
            var whatTimeJobDetail = JobBuilder.Create<WhatTimeJob>().SetJobData(data).Build();
            var whatTimeTrigger = TriggerBuilder.Create().StartNow().WithCronSchedule(WhatTimeCron).Build();

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
    public void SendTestMessage()
    {
        _queue.Enqueue(new TextMessage("Test message", "Wechat has been attached successfully."));
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
        _queue.Enqueue(new TextMessage("Waka Time", message));
    }

    /// <summary>
    /// Send the weather message.
    /// </summary>
    [RelayCommand]
    public async Task SendWeatherMessageAsync()
    {
        var message = await _container.Resolve<WeatherService>().GetMessageAsync();
        _queue.Enqueue(new TextMessage("Weather", message));
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

        _queue.Enqueue(new TextMessage("Daily News", message));
    }

    /// <summary>
    /// Send What Time is is message.
    /// </summary>
    [RelayCommand]
    public async Task SendWhatTimeMessageAsync()
    {
        var file = $"{DateTime.Now:hh}.gif";
        var path = await WhatTimeJob.CacheGif(file);

        _queue.Enqueue(new FileMessage(file, path));
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
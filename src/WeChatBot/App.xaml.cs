using Prism.Ioc;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Windows;
using WeChatBot.Services;
using WeChatBot.Views;

namespace WeChatBot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log-.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .CreateLogger();

        containerRegistry.RegisterSingleton<ILogger>(() => Log.Logger);
        containerRegistry.RegisterSingleton<AutomateService>();
        containerRegistry.RegisterSingleton<DailyNewsService>();
        containerRegistry.RegisterSingleton<WakaTimeService>();
        containerRegistry.RegisterSingleton<WeatherService>();
        containerRegistry.RegisterSingleton<HttpClient>(() => new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        {
            Timeout = TimeSpan.FromMinutes(3)
        });
    }

    /// <inheritdoc />
    protected override Window CreateShell() => Container.Resolve<MainView>();
}
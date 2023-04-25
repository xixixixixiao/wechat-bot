using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WeChatBot.ViewModels;

[INotifyPropertyChanged]
public partial class MainViewModel
{
    /// <summary>
    /// The log text.
    /// </summary>
    [ObservableProperty]
    private string _log;

    /// <summary>
    /// Whether to enable sending Waka Time message.
    /// </summary>
    [ObservableProperty]
    private bool _enableWakaTime;

    /// <summary>
    /// The moment when sending the Waka Time message.
    /// </summary>
    [ObservableProperty]
    private string _wakaTimeMoment;

    /// <summary>
    /// Whether to enable sending weather forecast messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableWeather;

    /// <summary>
    /// The moment when sending the weather forecast message.
    /// </summary>
    [ObservableProperty]
    private string _weatherMoment;

    /// <summary>
    /// Whether to enable sending Daily News messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableDailyNews;

    /// <summary>
    /// The moment when sending the Daily News message.
    /// </summary>
    [ObservableProperty]
    private string _dailyNewsMoment;

    /// <summary>
    /// The cities for which the weather forecast will be sent.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _cities;

    /// <summary>
    /// Attach the Wechat.exe.
    /// </summary>
    [RelayCommand]
    public async Task AttachWechatAsync()
    {
    }

    /// <summary>
    /// Send a test message.
    /// </summary>
    [RelayCommand]
    public async Task SendTestMessageAsync()
    {
    }

    /// <summary>
    /// Send the leaderboard message of Waka Time.
    /// </summary>
    [RelayCommand]
    public async Task SendWakaTimeMessageAsync()
    {
    }

    /// <summary>
    /// Send the weather message.
    /// </summary>
    [RelayCommand]
    public async Task SendWeatherMessageAsync()
    {
    }

    /// <summary>
    /// Send the Daily News message.
    /// </summary>
    [RelayCommand]
    public async Task SendDailyNewsMessageAsync()
    {
    }
}
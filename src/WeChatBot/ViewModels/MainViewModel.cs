using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using WeChatBot.Models.DailyNews;
using WeChatBot.Models.WakaTime;
using WeChatBot.Models.Weathers;

namespace WeChatBot.ViewModels;

[INotifyPropertyChanged]
public partial class MainViewModel
{
    private static readonly HttpClient HttpClient;
    private static FlaUI.Core.Application _application;

    static MainViewModel()
    {
        HttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        {
            Timeout = TimeSpan.FromMinutes(3)
        };
    }

    private readonly Timer _timer = new(TimeSpan.FromSeconds(40));

    public MainViewModel()
    {
        Cities.AddRange(new[]
        {
            new WeatherCity("北京", "110000"),
            new WeatherCity("上海", "310000"),
            new WeatherCity("深圳", "440300"),
            new WeatherCity("武汉", "420100"),
            new WeatherCity("杭州", "330100"),
            new WeatherCity("厦门", "350200"),
        });
        _timer.Elapsed += TimerOnElapsed;
    }

    private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        var hour = now.Hour;
        var minute = now.Minute;

        if (EnableWakaTime)
        {
            var wakaTimeMoment = TimeSpan.Parse(WakaTimeMoment);

            if (hour == wakaTimeMoment.Hours && minute == wakaTimeMoment.Minutes)
            {
                try
                {
                    var message = await GetWakaTimeMessageAsync();
                    await SendMessageAsync(message);
                }
                catch (Exception exception)
                {
                    Error(exception.Message);
                }
            }
        }

        if (EnableWeather)
        {
            var weatherMoment = TimeSpan.Parse(WeatherMoment);

            if (hour == weatherMoment.Hours && minute == weatherMoment.Minutes)
            {
                try
                {
                    var message = await GetWeatherMessageAsync();
                    await SendMessageAsync(message);
                }
                catch (Exception exception)
                {
                    Error(exception.Message);
                }
            }
        }

        if (EnableDailyNews)
        {
            var dailyNewsMoment = TimeSpan.Parse(DailyNewsMoment);

            if (hour == dailyNewsMoment.Hours && minute == dailyNewsMoment.Minutes)
            {
                try
                {
                    var message = await GetDailyNewsMessageAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        Error("There is no Daily News right now.");
                    }
                    else
                    {
                        await SendMessageAsync(message);
                    }
                }
                catch (Exception exception)
                {
                    Error(exception.Message);
                }
            }
        }
    }

    /// <summary>
    /// The log text.
    /// </summary>
    [ObservableProperty]
    private string _log = string.Empty;

    /// <summary>
    /// Whether to enable sending Waka Time message.
    /// </summary>
    [ObservableProperty]
    private bool _enableWakaTime = true;

    /// <summary>
    /// The moment when sending the Waka Time message.
    /// </summary>
    [ObservableProperty]
    private string _wakaTimeMoment = "09:30:00";

    /// <summary>
    /// Whether to enable sending weather forecast messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableWeather = true;

    /// <summary>
    /// The moment when sending the weather forecast message.
    /// </summary>
    [ObservableProperty]
    private string _weatherMoment = "08:30:00";

    /// <summary>
    /// Whether to enable sending Daily News messages.
    /// </summary>
    [ObservableProperty]
    private bool _enableDailyNews = true;

    /// <summary>
    /// The moment when sending the Daily News message.
    /// </summary>
    [ObservableProperty]
    private string _dailyNewsMoment = "09:00:00";

    /// <summary>
    /// The cities for which the weather forecast will be sent.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WeatherCity> _cities = new();

    /// <summary>
    /// Attach the Wechat.exe.
    /// </summary>
    [RelayCommand]
    public void AttachWechat()
    {
        if (_application != null)
        {
            NotifyInfo("Wechat already has been attached.");
            return;
        }

        _application = FlaUI.Core.Application.Attach("WeChat.exe");

        if (_application is null)
        {
            NotifyWarning("Unable to attach WeChat.");
        }
        else
        {
            NotifyInfo("Wechat has been attached successfully.");
        }
    }

    /// <summary>
    /// Start the task.
    /// </summary>
    [RelayCommand]
    public void StartTask()
    {
        if (_application is null)
        {
            NotifyWarning("Does not attach Wechat.");
            return;
        }

        if (!_timer.Enabled)
        {
            _timer.Start();
            NotifyInfo("Started successfully.");
        }
    }

    /// <summary>
    /// Stop the task.
    /// </summary>
    [RelayCommand]
    public void StopTask()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            NotifyInfo("The task has been stopped.");
        }
    }

    /// <summary>
    /// Send a test message.
    /// </summary>
    [RelayCommand]
    public async Task SendTestMessageAsync()
    {
        if (_application is null)
        {
            NotifyWarning("Does not attach Wechat.");
            return;
        }

        await SendMessageAsync("Wechat has been attached successfully.");
    }

    /// <summary>
    /// Send the leaderboard message of Waka Time.
    /// </summary>
    [RelayCommand]
    public async Task SendWakaTimeMessageAsync()
    {
        if (_application is null)
        {
            NotifyWarning("Does not attach Wechat.");
            return;
        }

        var message = await GetWakaTimeMessageAsync();
        await SendMessageAsync(message);
    }

    /// <summary>
    /// Send the weather message.
    /// </summary>
    [RelayCommand]
    public async Task SendWeatherMessageAsync()
    {
        if (_application is null)
        {
            NotifyWarning("Does not attach Wechat.");
            return;
        }

        var message = await GetWeatherMessageAsync();
        await SendMessageAsync(message);
    }

    /// <summary>
    /// Send the Daily News message.
    /// </summary>
    [RelayCommand]
    public async Task SendDailyNewsMessageAsync()
    {
        if (_application is null)
        {
            NotifyWarning("Does not attach Wechat.");
            return;
        }

        var message = await GetDailyNewsMessageAsync();
        if (string.IsNullOrEmpty(message))
        {
            NotifyInfo("There is no Daily News right now.");
            return;
        }

        await SendMessageAsync(message);
    }

    /// <summary>
    /// Send message to Wechat.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task SendMessageAsync(string message)
    {
        var count = 0;
        while (count++ <= 5)
        {
            using var automation = new UIA3Automation();
            var window = _application.GetMainWindow(automation);

            var messageTextBox = window.FindFirstDescendant(cf => cf.ByName("Enter")).AsTextBox();
            var sendMessageButton = window
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .FirstOrDefault(control => control.Name == "sendBtn");

            if (messageTextBox is null || sendMessageButton is null)
            {
                Warning("Cannot find the Message box.");
                Warning("Cannot find the `Send` button.");
                await Task.Delay(TimeSpan.FromSeconds(5));
                continue;
            }

            messageTextBox.Focus();
            messageTextBox.Click();
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
            messageTextBox.Enter(message);
            await Task.Delay(TimeSpan.FromSeconds(2));
            sendMessageButton.Click();
            return;
        }
    }

    /// <summary>
    /// Get Waka Time message.
    /// </summary>
    /// <returns>The Waka Time message.</returns>
    private async Task<string> GetWakaTimeMessageAsync()
    {
        // IT 领袖
        // 码魔：xxx
        // 这周：xx hrs xx mins
        // 每天：xx hrs
        // 
        // ...
        // 
        // 这辈子都不可能打工
        // 码佛：xxx
        // 这周：xx hrs xx mins
        // 每天：xx mins
        var titles = new List<string>
        {
            "IT 领袖", "IT 大哥", "IT 精英", "IT 人才", "IT 工程师",
            "摸鱼", "划水", "打酱油", "打工是不可能打工", "这辈子都不可能打工",
        };
        var levels = new List<string>
        {
            "魔", "鬼", "怪", "妖", "狂",
            "灵", "圣", "仙", "神", "佛",
        };

        string GenerateMessage(Rank rank)
        {
            return $"{titles[rank.Index - 1]}\r\n" +
                   $"码{levels[rank.Index - 1]}：{rank.User.DisplayName}\r\n" +
                   $"这周：{rank.RunningTotal.Total}\r\n" +
                   $"每天：{rank.RunningTotal.Average}";
        }

        // App ID
        // 305280iy5v8jBuiKiGxaE4Ga
        // App Secret
        // waka_sec_yhnhJnRtfHWyZjfqnJuKQSeqKSOF9aJnW0bT5Vhb0xlFSUkSM39owXC2TYieU2RcZB0Ni4lg6bDdQ3uR
        // Your secret api key
        // waka_fd6ecf87-8e30-4399-b7b1-10889627be41
        //
        const string secret = "waka_fd6ecf87-8e30-4399-b7b1-10889627be41";
        const string baseUrl = "https://wakatime.com";
        const string path = "/api/v1/users/current/leaderboards/";
        const string id = "47261ca3-db4e-4055-9954-a324de62c618";
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + id);
        request.Headers.Add("Authorization", "Basic " + Base64(secret));
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonConvert.DeserializeObject<Leaderboard>(content);

        return string.Join("\r\n\r\n", leaderboard.Ranks.Take(10).Select(GenerateMessage));
    }

    /// <summary>
    /// Get weather message.
    /// </summary>
    /// <returns>The weather message.</returns>
    private async Task<string> GetWeatherMessageAsync()
    {
        // Get weather forecast from AMAP.
        // https://lbs.amap.com/api/webservice/guide/api/weatherinfo
        //
        string GenerateMessage(WeatherResult result)
        {
            // 武汉 14℃ 晴 || 小雨
            // 【下雨预警】武汉市的小伙伴别忘了带伞。
            var live = result.Lives.First();
            var umbrella = live.Weather.Contains("雨") ? $"\r\n【下雨预警】{live.City}的小伙伴别忘了带伞。" : "";
            return $"{live.City} {live.Temperature}℃ {live.Weather}" + umbrella;
        }

        const string secret = "ac863da4ebff9b4d23556db60703e52c";
        const string baseUrl = "https://restapi.amap.com";
        const string path = "/v3/weather/weatherInfo";
        var results = new List<WeatherResult>();

        foreach (var city in Cities)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + $"?key={secret}&city={city.Code}");
            var response = await HttpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            results.Add(JsonConvert.DeserializeObject<WeatherResult>(content));
        }

        return string.Join("\r\n", results.Select(GenerateMessage));
    }

    /// <summary>
    /// Get Daily News message.
    ///
    /// https://www.163.com/dy/media/T1603594732083.html
    /// Get Daily New from 163.
    ///
    /// 1. Get the list of all news for the current page.
    /// 2. Get the latest news.
    ///    Compare the date of the latest news and the date of today.
    ///    a. Send the latest news if the date of the latest news is today's.
    ///    b. Otherwise, check whether exists the latest news every five minutes.
    /// </summary>
    /// <returns>The Daily News message.</returns>
    private async Task<string> GetDailyNewsMessageAsync()
    {
        var dailyNewsItems = await GetDailyNewsItemsAsync();
        var latestNews = dailyNewsItems.MaxBy(news => news.PublishTime);

        var today = DateTime.Today;
        var latest = latestNews.PublishTime;
        if (latest.Year == today.Year && latest.Month == today.Month && latest.Day == today.Day)
        {
            var article = await GetArticleAsync(latestNews.Url);
            return article;
        }

        DailyNewsMoment = $"{TimeSpan.Parse(DailyNewsMoment).Add(TimeSpan.FromMinutes(5)):hh\\:mm\\:ss}";
        return string.Empty;
    }

    #region 163 News

    /// <summary>
    /// Get the article of the news.
    /// </summary>
    /// <param name="url">The url of news.</param>
    /// <returns>The article of the news.</returns>
    private static async Task<string> GetArticleAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddFirefoxHeaders(request);
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var postNode = document.DocumentNode
            .SelectSingleNode("//div[@class='post_body']")
            .Descendants("p")
            .FirstOrDefault(p => p.InnerText.Contains("每天一分钟，知晓天下事！"));
        var article = postNode?.InnerHtml
            .Trim()
            .Replace("<br>", "\r\n")
            .Replace("公众号：365资讯简报", string.Empty);

        if (article is null)
        {
            return null;
        }

        const string header = "知晓天下事！";
        const string footer = "【微语】";
        var first = article.IndexOf(header, StringComparison.Ordinal);
        var last = article.LastIndexOf(footer, StringComparison.Ordinal);

        return article[(first + header.Length)..last].Trim();
    }

    /// <summary>
    /// Get the list of all news for the current page.
    /// </summary>
    /// <returns>The list of all news</returns>
    private static async Task<List<NewsItem>> GetDailyNewsItemsAsync()
    {
        const string baseUrl = "https://www.163.com";
        const string listPath = "/dy/media/T1603594732083.html";
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + listPath);
        AddFirefoxHeaders(request);
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var nodes = document.DocumentNode.SelectNodes("//li[@class='js-item item']").ToList();
        var items = new List<NewsItem>();

        foreach (var node in nodes)
        {
            var link = node.Descendants("h4").First().Descendants("a").First();
            var title = link.InnerText;
            var url = link.GetAttributeValue("href", string.Empty);
            var date = node.Descendants("span").First().InnerText;
            items.Add(new NewsItem
            {
                Title = title,
                Url = url,
                PublishTime = DateTime.Parse(date)
            });
        }

        return items;
    }

    #endregion

    #region Utilities

    private static void AddFirefoxHeaders(HttpRequestMessage request)
    {
        const string firefox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0";

        request.Headers.Add("Accept", "text/html,application/xhtml+xml");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");
        request.Headers.Add("User-Agent", firefox);
    }

    private static string Base64(string input)
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(input));
    }

    #endregion

    #region Log

    private static void NotifyInfo(string message)
    {
        MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void NotifyWarning(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void Debug(string message)
    {
        CheckLogLength();
        WriteLog(message, "DBG");
    }

    private void Info(string message)
    {
        CheckLogLength();
        WriteLog(message, "INF");
    }

    private void Warning(string message)
    {
        CheckLogLength();
        WriteLog(message, "WRN");
    }

    private void Error(string message)
    {
        CheckLogLength();
        WriteLog(message, "ERR");
    }

    private void CheckLogLength()
    {
        if (Log.Length >= 2_000)
        {
            Log = string.Empty;
        }
    }

    private void WriteLog(string message, string level)
    {
        Log += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}\r\n";
    }

    #endregion
}
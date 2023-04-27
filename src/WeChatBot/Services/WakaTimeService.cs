using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WeChatBot.Models.WakaTime;

namespace WeChatBot.Services;

public class WakaTimeService
{
    private readonly HttpClient _http;

    public WakaTimeService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Get Waka Time message.
    /// </summary>
    /// <returns>The Waka Time message.</returns>
    public async Task<string> GetMessageAsync()
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

        // Your secret api key
        // waka_00000000-0000-0000-0000-000000000000
        const string secret = Constants.WAKA_TIME_SECRET;
        const string baseUrl = "https://wakatime.com";
        const string path = "/api/v1/users/current/leaderboards/";
        const string id = Constants.WAKA_TIME_LEADERBOARD;
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + id);
        request.Headers.Add("Authorization", "Basic " + Base64(secret));
        var response = await _http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonConvert.DeserializeObject<Leaderboard>(content);

        return string.Join("\r\n\r\n", leaderboard.Ranks.Take(10).Select(GenerateMessage));
    }

    private static string Base64(string input)
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(input));
    }
}
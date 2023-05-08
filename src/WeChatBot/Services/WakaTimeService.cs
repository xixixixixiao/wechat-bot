using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WeChatBot.Models.WakaTime;

namespace WeChatBot.Services;

public class WakaTimeService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public WakaTimeService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
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
        // 语言：Java
        // 
        // ...
        // 
        // 这辈子都不可能打工
        // 码佛：xxx
        // 这周：xx hrs xx mins
        // 每天：xx mins
        // 语言：Java
        var titles = new List<string>
        {
            "IT 领袖", "IT 大哥", "IT 精英", "IT 人才", "IT 工程师",
            "摸鱼", "划水", "打酱油", "打工是不可能打工", "这辈子都不可能打工",
        };

        string GenerateMessage(Rank rank)
        {
            return $"{titles[rank.Index - 1]}\r\n" +
                   $"码农：{rank.User.DisplayName}\r\n" +
                   $"这周：{rank.RunningTotal.Total}\r\n" +
                   $"每天：{rank.RunningTotal.Average}\r\n" +
                   $"语言：{rank.RunningTotal.Languages.MaxBy(language => language.TotalSeconds)?.Name}";
        }

        // Your secret api key
        // waka_00000000-0000-0000-0000-000000000000
        const string baseUrl = "https://wakatime.com";
        const string path = "/api/v1/users/current/leaderboards/";

        var id = _configuration["WakaTime:Leaderboard"];
        var secret = _configuration["WakaTime:Secret"];

        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + id);
        request.Headers.Add("Authorization", "Basic " + Base64(secret));
        var response = await _http.SendAsync(request);
        var leaderboard = await response.Content.ReadFromJsonAsync<Leaderboard>();

        return string.Join("\r\n\r\n", leaderboard.Ranks.Take(10).Select(GenerateMessage));
    }

    private static string Base64(string input)
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(input));
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeChatBot.Models.Weathers;

namespace WeChatBot.Services;

public class WeatherService
{
    private readonly HttpClient _http;

    /// <summary>
    /// The cities for which the weather forecast will be sent.
    /// </summary>
    private List<WeatherCity> _cities = new();

    public WeatherService(HttpClient http)
    {
        _http = http;
        _cities.AddRange(new[]
        {
            new WeatherCity("北京", "110000"),
            new WeatherCity("上海", "310000"),
            new WeatherCity("深圳", "440300"),
            new WeatherCity("武汉", "420100"),
            new WeatherCity("杭州", "330100"),
            new WeatherCity("厦门", "350200"),
        });
    }

    /// <summary>
    /// Get weather message.
    /// </summary>
    /// <returns>The weather message.</returns>
    public async Task<string> GetMessageAsync()
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

        foreach (var city in _cities)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + $"?key={secret}&city={city.Code}");
            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            results.Add(JsonConvert.DeserializeObject<WeatherResult>(content));
        }

        return string.Join("\r\n", results.Select(GenerateMessage));
    }
}
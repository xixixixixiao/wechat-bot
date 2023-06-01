using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WeChatBot.Models.Weathers;

namespace WeChatBot.Services;

public class WeatherService
{
    private static List<WeatherCity> _weatherCities = new();

    private const string BaseUrl = "https://restapi.amap.com";
    private const string Path = "/v3/weather/weatherInfo";

    private readonly HttpClient _http;
    private readonly string _secret;
    private readonly string[] _cities; // The cities for which the weather forecast will be sent.

    public WeatherService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _secret = configuration["Weather:Secret"];
        _cities = configuration.GetSection("Weather:Cities").Get<string[]>();
    }

    /// <summary>
    /// Get weather message.
    ///
    /// Get weather forecast from AMAP:
    /// https://lbs.amap.com/api/webservice/guide/api/weatherinfo
    /// </summary>
    /// <returns>The weather message.</returns>
    public async Task<string> GetMessageAsync()
    {
        if (!_weatherCities.Any())
        {
            _weatherCities = await GetWeatherCitiesAsync();
        }

        var messageBuilder = new StringBuilder();

        foreach (var city in _cities)
        {
            var code = _weatherCities.FirstOrDefault(wc => wc.City == city)?.Code;

            // Live weather.
            var liveUrl = BaseUrl + Path + $"?key={_secret}&city={code}";
            var liveRequest = new HttpRequestMessage(HttpMethod.Get, liveUrl);
            var liveResponse = await _http.SendAsync(liveRequest);
            var liveWeatherResult = await liveResponse.Content.ReadFromJsonAsync<LiveWeatherResult>();
            var liveWeather = liveWeatherResult.Lives.FirstOrDefault();

            // Forecast weather.
            var forecastUrl = BaseUrl + Path + $"?key={_secret}&city={code}&extensions=all";
            var forecastRequest = new HttpRequestMessage(HttpMethod.Get, forecastUrl);
            var forecastResponse = await _http.SendAsync(forecastRequest);
            var forecastWeatherResult = await forecastResponse.Content.ReadFromJsonAsync<ForecastWeatherResult>();
            var forecastWeather = forecastWeatherResult.ForecastWeathers.FirstOrDefault();

            var message = GenerateMessage(liveWeather, forecastWeather);

            messageBuilder
                .AppendLine(message)
                .AppendLine();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
#endif

            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }

        return messageBuilder.ToString().Trim();
    }

    private static string TemperatureToEmoji(string temperature)
    {
        // -35℃ ~ -5℃ 极冷 🤪
        //  -5℃ ~  5℃ 很冷 🥶
        //   5℃ ~ 15℃ 冷   😬
        //  15℃ ~ 25℃ 舒适 😇
        //  25℃ ~ 35℃ 热   🥵
        //  35℃ ~ 50℃ 炙热 🔥
        return Convert.ToInt32(temperature) switch
        {
            < -5 => "🤪",
            < 5 => "🥶",
            < 15 => "😬",
            < 25 => "😇",
            < 35 => "🥵",
            _ => "🔥"
        };
    }

    private static string WeatherToEmoji(string weather)
    {
        var dict = new Dictionary<string, string>
        {
            ["晴"] = "☀️",
            ["少云"] = "🌤️",
            ["晴间多云"] = "🌤️",
            ["多云"] = "🌥️",
            ["阴"] = "⛅",
            ["有风"] = "🪁⛱️",
            ["平静"] = "⛱️",
            ["微风"] = "🌈⛱️",
            ["和风"] = "🪁🌈",
            ["清风"] = "🪁🌈",
            ["强风/劲风"] = "🌀",
            ["疾风"] = "🌀🌀",
            ["大风"] = "🌀🌀",
            ["烈风"] = "🌀🌀🌀",
            ["风暴"] = "🈲🌀🌀🌀",
            ["狂爆风"] = "🈲🌀🌀🌀🌀",
            ["飓风"] = "🈲🌀🌀🌀🌀🌀",
            ["热带风暴"] = "🌪️🌪️",
            ["霾"] = "🌫️",
            ["中度霾"] = "🌫️🌫️",
            ["重度霾"] = "🌫️🌫️🌫️",
            ["严重霾"] = "🌫️🌫️🌫️",
            ["阵雨"] = "🌦️",
            ["雷阵雨"] = "⛈️",
            ["雷阵雨并伴有冰雹"] = "⛈️🧊",
            ["小雨"] = "🌧️",
            ["中雨"] = "🌧️🌧️",
            ["大雨"] = "🌧️🌧️🌧️",
            ["暴雨"] = "🌧️🌧️🌧️🌧️",
            ["大暴雨"] = "🈲🌧️🌧️🌧️🌧️🌧️",
            ["特大暴雨"] = "🈲🌧️🌧️🌧️🌧️🌧️🌧️",
            ["强阵雨"] = "⛈️⛈️",
            ["强雷阵雨"] = "⛈️⛈️⚡⚡",
            ["极端降雨"] = "🈲🈲🌧️🌧️🌧️🌧️🌧️",
            ["毛毛雨/细雨"] = "🌧️",
            ["雨"] = "🌧️",
            ["小雨-中雨"] = "🌧️",
            ["中雨-大雨"] = "🌧️🌧️",
            ["大雨-暴雨"] = "🌧️🌧️🌧️",
            ["暴雨-大暴雨"] = "🌧️🌧️🌧️",
            ["大暴雨-特大暴雨"] = "🈲🌧️🌧️🌧️🌧️",
            ["雨雪天气"] = "🌧️❄️",
            ["雨夹雪"] = "🌧️❄️",
            ["阵雨夹雪"] = "⛈️❄️",
            ["冻雨"] = "❄️",
            ["雪"] = "❄️",
            ["阵雪"] = "❄️",
            ["小雪"] = "❄️",
            ["中雪"] = "❄️❄️",
            ["大雪"] = "❄️❄️❄️",
            ["暴雪"] = "❄️❄️❄️❄️",
            ["小雪-中雪"] = "❄️",
            ["中雪-大雪"] = "❄️❄️",
            ["大雪-暴雪"] = "❄️❄️❄️",
            ["浮尘"] = "🌪️",
            ["扬沙"] = "🌪️",
            ["沙尘暴"] = "🌪️",
            ["强沙尘暴"] = "🈲🌪️🌪️",
            ["龙卷风"] = "🈲🌪️🌪️🌪️",
            ["雾"] = "🌫️",
            ["浓雾"] = "🌫️🌫️",
            ["强浓雾"] = "🌫️🌫️🌫️",
            ["轻雾"] = "🌫️",
            ["大雾"] = "🌫️🌫️",
            ["特强浓雾"] = "🌫️🌫️🌫️🌫️",
            ["热"] = "🔥",
            ["冷"] = "🥶",
            ["未知"] = "⛱️"
        };

        return dict[weather];
    }

    private static string GenerateMessage(LiveWeather live, ForecastWeather forecast)
    {
        // Template:
        // 武汉：晴14℃🔥【☔】
        // 预报：晴️🥵小雨️🥵小雨🥵
        var message = new StringBuilder();
        var casts = forecast
            .Forecasts
            .Skip(1)
            .Select(fore => WeatherToEmoji(fore.DayWeather) + TemperatureToEmoji(fore.DayTemperature));

        message
            .Append(live.City.Replace("市", ""))
            .Append("：")
            .Append(live.Weather)
            .Append(live.Temperature)
            .Append("℃")
            .Append(TemperatureToEmoji(live.Temperature))
            .Append(live.Weather.Contains("雨") ? "【☔】" : "")
            .AppendLine()
            .Append("预报：")
            .Append(string.Join("~", casts));

        return message.ToString();
    }

    private static async Task<List<WeatherCity>> GetWeatherCitiesAsync()
    {
        var codeUri = new Uri("Assets/weather_code.csv", UriKind.Relative);

        await using var stream = Application.GetResourceStream(codeUri)?.Stream ?? Stream.Null;
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = "\t",
        });

        return csvReader.GetRecords<WeatherCity>().ToList();
    }
}
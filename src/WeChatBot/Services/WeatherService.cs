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
using System.Threading.Tasks;
using System.Windows;
using WeChatBot.Models.Weathers;

namespace WeChatBot.Services;

public class WeatherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public WeatherService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
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
        string GenerateMessage(WeatherResult result)
        {
            // 武汉 14℃ 晴 || 小雨
            // 【下雨预警】武汉市的小伙伴别忘了带伞。
            var live = result.Lives.First();
            var umbrella = live.Weather.Contains("雨") ? $"\r\n【下雨预警】{live.City}的小伙伴别忘了带伞。" : "";
            return $"{live.City} {live.Temperature}℃ {live.Weather}" + umbrella;
        }

        const string baseUrl = "https://restapi.amap.com";
        const string path = "/v3/weather/weatherInfo";

        var results = new List<WeatherResult>();
        var secret = _configuration["Weather:Secret"];
        // The cities for which the weather forecast will be sent.
        var cities = _configuration.GetSection("Weather:Cities").Get<string[]>();
        var weatherCities = await GetWeatherCitiesAsync();

        foreach (var city in cities)
        {
            var code = weatherCities.FirstOrDefault(wc => wc.City == city)?.Code;
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path + $"?key={secret}&city={code}");
            var response = await _http.SendAsync(request);
            var weatherResult = await response.Content.ReadFromJsonAsync<WeatherResult>();

            results.Add(weatherResult);
        }

        return string.Join("\r\n", results.Select(GenerateMessage));
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
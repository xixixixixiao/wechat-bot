using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class ForecastWeatherResult
{
    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("forecasts")]
    public ForecastWeather[] ForecastWeathers { get; set; }
}
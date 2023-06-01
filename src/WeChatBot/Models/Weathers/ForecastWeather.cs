using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class ForecastWeather
{
    [JsonPropertyName("casts")]
    public Forecast[] Forecasts { get; set; }
}
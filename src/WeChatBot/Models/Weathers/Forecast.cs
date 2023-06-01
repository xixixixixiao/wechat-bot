using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class Forecast
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("dayweather")]
    public string DayWeather { get; set; }

    [JsonPropertyName("daytemp")]
    public string DayTemperature { get; set; }

    [JsonPropertyName("nightweather")]
    public string NightWeather { get; set; }

    [JsonPropertyName("nighttemp")]
    public string NightTemperature { get; set; }
}
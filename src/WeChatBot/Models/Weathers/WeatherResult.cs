using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class WeatherResult
{
    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("lives")]
    public WeatherLive[] Lives { get; set; }
}
using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class LiveWeatherResult
{
    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("lives")]
    public LiveWeather[] Lives { get; set; }
}
using Newtonsoft.Json;

namespace WeChatBot.Models.Weathers;

public class WeatherResult
{
    [JsonProperty("info")]
    public string Info { get; set; }

    [JsonProperty("lives")]
    public WeatherLive[] Lives { get; set; }
}
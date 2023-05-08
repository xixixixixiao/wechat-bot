using System.Text.Json.Serialization;

namespace WeChatBot.Models.Weathers;

public class WeatherLive
{
    [JsonPropertyName("province")]
    public string Province { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("weather")]
    public string Weather { get; set; }

    [JsonPropertyName("temperature")]
    public string Temperature { get; set; }
}
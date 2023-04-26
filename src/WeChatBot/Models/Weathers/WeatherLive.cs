using Newtonsoft.Json;

namespace WeChatBot.Models.Weathers;

public class WeatherLive
{
    [JsonProperty("province")]
    public string Province { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("weather")]
    public string Weather { get; set; }

    [JsonProperty("temperature")]
    public string Temperature { get; set; }
}
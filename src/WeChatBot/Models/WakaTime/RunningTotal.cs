using Newtonsoft.Json;

namespace WeChatBot.Models.WakaTime;

public class RunningTotal
{
    [JsonProperty("human_readable_total")]
    public string Total { get; set; }

    [JsonProperty("human_readable_daily_average")]
    public string Average { get; set; }
}
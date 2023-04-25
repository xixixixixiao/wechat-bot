using Newtonsoft.Json;

namespace WeChatBot.Models.WakaTime;

public class Rank
{
    [JsonProperty("rank")]
    public int Index { get; set; }

    [JsonProperty("running_total")]
    public RunningTotal RunningTotal { get; set; }

    [JsonProperty("user")]
    public User User { get; set; }
}
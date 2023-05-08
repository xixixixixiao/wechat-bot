using System.Text.Json.Serialization;

namespace WeChatBot.Models.WakaTime;

public class Rank
{
    [JsonPropertyName("rank")]
    public int Index { get; set; }

    [JsonPropertyName("running_total")]
    public RunningTotal RunningTotal { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeChatBot.Models.WakaTime;

public class Leaderboard
{
    [JsonPropertyName("data")]
    public List<Rank> Ranks { get; set; }
}
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeChatBot.Models.WakaTime;

public class Leaderboard
{
    [JsonProperty("data")]
    public List<Rank> Ranks { get; set; }
}
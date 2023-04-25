using Newtonsoft.Json;

namespace WeChatBot.Models.WakaTime;

public class User
{
    [JsonProperty("display_name")]
    public string DisplayName { get; set; }
}
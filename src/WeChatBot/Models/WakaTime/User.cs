using System.Text.Json.Serialization;

namespace WeChatBot.Models.WakaTime;

public class User
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
}
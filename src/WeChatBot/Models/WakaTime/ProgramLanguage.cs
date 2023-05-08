using System.Text.Json.Serialization;

namespace WeChatBot.Models.WakaTime;

public class ProgramLanguage
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("total_seconds")]
    public double TotalSeconds { get; set; }
}
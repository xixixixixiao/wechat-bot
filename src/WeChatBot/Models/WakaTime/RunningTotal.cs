using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeChatBot.Models.WakaTime;

public class RunningTotal
{
    [JsonPropertyName("human_readable_total")]
    public string Total { get; set; }

    [JsonPropertyName("human_readable_daily_average")]
    public string Average { get; set; }

    [JsonPropertyName("languages")]
    public List<ProgramLanguage> Languages { get; set; }
}
using CsvHelper.Configuration.Attributes;

namespace WeChatBot.Models.Weathers;

public class WeatherCity
{
    [Index(0)]
    public string City { get; set; }

    [Index(1)]
    public string Code { get; set; }
}
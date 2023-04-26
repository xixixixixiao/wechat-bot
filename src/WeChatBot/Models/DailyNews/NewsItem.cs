using System;

namespace WeChatBot.Models.DailyNews;

public class NewsItem
{
    public string Title { get; set; }
    public DateTime PublishTime { get; set; }
    public string Url { get; set; }
}
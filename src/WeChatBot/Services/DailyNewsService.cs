using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeChatBot.Models.DailyNews;

namespace WeChatBot.Services;

public class DailyNewsService
{
    private readonly HttpClient _http;

    public DailyNewsService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Get Daily News message.
    ///
    /// https://www.163.com/dy/media/T1603594732083.html
    /// Get Daily New from 163.
    ///
    /// 1. Get the list of all news for the current page.
    /// 2. Get the latest news.
    ///    Compare the date of the latest news and the date of today.
    ///    a. Send the latest news if the date of the latest news is today's.
    ///    b. Otherwise, check whether exists the latest news every five minutes.
    /// </summary>
    /// <returns>The Daily News message.</returns>
    public async Task<string> GetMessageAsync()
    {
        var dailyNewsItems = await GetDailyNewsItemsAsync();
        var latestNews = dailyNewsItems.MaxBy(news => news.PublishTime);

        var today = DateTime.Today;
        var latest = latestNews.PublishTime;
        if (latest.Year == today.Year &&
            latest.Month == today.Month &&
            latest.Day == today.Day)
        {
            return await GetArticleAsync(latestNews.Url);
        }

        return string.Empty;
    }

    private static void AddFirefoxHeaders(HttpRequestMessage request)
    {
        const string firefox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0";

        request.Headers.Add("Accept", "text/html,application/xhtml+xml");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");
        request.Headers.Add("User-Agent", firefox);
    }


    #region 163 News

    /// <summary>
    /// Get the article of the news.
    /// </summary>
    /// <param name="url">The url of news.</param>
    /// <returns>The article of the news.</returns>
    private async Task<string> GetArticleAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddFirefoxHeaders(request);
        var response = await _http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var postNode = document.DocumentNode
            .SelectSingleNode("//div[@class='post_body']")
            .Descendants("p")
            .FirstOrDefault(p => p.InnerText.Contains("每天一分钟，知晓天下事！"));
        var article = postNode?.InnerHtml
            .Trim()
            .Replace("<br>", "\r\n")
            .Replace("公众号：365资讯简报", string.Empty);

        if (article is null)
        {
            return null;
        }

        const string header = "知晓天下事！";
        const string footer = "【微语】";
        var first = article.IndexOf(header, StringComparison.Ordinal);
        var last = article.LastIndexOf(footer, StringComparison.Ordinal);

        return article[(first + header.Length)..last].Trim();
    }

    /// <summary>
    /// Get the list of all news for the current page.
    /// </summary>
    /// <returns>The list of all news</returns>
    private async Task<List<NewsItem>> GetDailyNewsItemsAsync()
    {
        const string baseUrl = "https://www.163.com";
        const string listPath = "/dy/media/T1603594732083.html";
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + listPath);
        AddFirefoxHeaders(request);
        var response = await _http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var nodes = document.DocumentNode.SelectNodes("//li[@class='js-item item']").ToList();
        var items = new List<NewsItem>();

        foreach (var node in nodes)
        {
            var link = node.Descendants("h4").First().Descendants("a").First();
            var title = link.InnerText;
            var url = link.GetAttributeValue("href", string.Empty);
            var date = node.Descendants("span").First().InnerText;
            items.Add(new NewsItem
            {
                Title = title,
                Url = url,
                PublishTime = DateTime.Parse(date)
            });
        }

        return items;
    }

    #endregion
}
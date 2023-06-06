using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeChatBot.Models.Animes;

namespace WeChatBot.Services;

public class AnimeService
{
    private readonly HttpClient _http;

    public AnimeService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetMessageAsync()
    {
        var title = "📋今日份动漫更新列表💦" + Environment.NewLine + Environment.NewLine;
        var list = await GetListAsync();

        return title + string.Join(
            Environment.NewLine + Environment.NewLine,
            list.Select(item => $"🎬【{item.Name}】\r\n⏰{item.Update}—{item.Episode}"));
    }

    public async Task<List<Anime>> GetListAsync()
    {
        var content = await GetPageAsync();
        var document = new HtmlDocument();

        document.LoadHtml(content);

        return document
            .DocumentNode
            .SelectNodes("//div[@class='lists-content list-week']")
            .Descendants("ul")
            .First()
            .ChildNodes
            .Where(node => node.Name == "li")
            .Select(li =>
            {
                var nodes = li.ChildNodes;

                var episode = nodes.FindFirst("a").InnerText.Trim();
                var name = nodes.FindFirst("h2").InnerText.Trim();
                var update = nodes.FindFirst("footer").InnerText.Trim().Replace("更新:", "");

                if (DateTime.TryParse(update, out var updateTime))
                {
                    update = updateTime.ToString("HH:mm");
                }

                return new Anime
                {
                    Episode = episode,
                    Name = name,
                    Update = update,
                };
            })
            .ToList();
    }

    public async Task<string> GetPageAsync()
    {
        const string url = "http://www.74fan.com/label/week.html";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _http.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }
}
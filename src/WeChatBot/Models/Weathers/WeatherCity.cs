namespace WeChatBot.Models.Weathers;

public class WeatherCity
{
    public string City { get; set; }
    public string Code { get; set; }

    public WeatherCity(string city, string code)
    {
        City = city;
        Code = code;
    }
}
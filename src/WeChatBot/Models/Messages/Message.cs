namespace WeChatBot.Models.Messages;

public abstract class Message
{
    public string Name { get; set; }

    protected Message(string name)
    {
        Name = name;
    }
}
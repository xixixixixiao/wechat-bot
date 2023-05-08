namespace WeChatBot.Models.Messages;

public class TextMessage : Message
{
    public string Text { get; set; }

    /// <inheritdoc />
    public TextMessage(string name, string text) : base(name)
    {
        Text = text;
    }
}
namespace WeChatBot.Models.Messages;

public class FileMessage : Message
{
    public string Path { get; set; }

    /// <inheritdoc />
    public FileMessage(string name, string path) : base(name)
    {
        Path = path;
    }
}
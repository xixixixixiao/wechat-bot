using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace WeChatBot.Services;

public class AutomateService : IDisposable
{
    private readonly ILogger _logger;

    private FlaUI.Core.Application _application;
    private UIA3Automation _automation;
    private FlaUI.Core.AutomationElements.Window _window;

    public AutomateService(ILogger logger)
    {
        _logger = logger;
    }

    public bool AttachWechat()
    {
        _application = FlaUI.Core.Application.Attach("WeChat.exe");
        _automation = new UIA3Automation();
        _window = _application.GetMainWindow(_automation);

        return _application is not null && _window is not null;
    }

    public FlaUI.Core.AutomationElements.TextBox GetMessageTextBox()
    {
        return FlaUI.Core
            .AutomationElements
            .AutomationElementExtensions
            .AsTextBox(_window.FindFirstDescendant(cf => cf.ByName("Enter")));
    }

    public FlaUI.Core.AutomationElements.Button GetSendMessageButton()
    {
        var element = _window
            .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
            .FirstOrDefault(control => control.Name == "sendBtn");
        return FlaUI.Core
            .AutomationElements
            .AutomationElementExtensions
            .AsButton(element);
    }

    /// <summary>
    /// Send a message to Wechat.
    /// </summary>
    /// <param name="message">The message will be sent to Wechat.</param>
    public async Task<bool> SendTextMessageAsync(string message)
    {
        await Task.CompletedTask;

        var messageTextBox = GetMessageTextBox();
        var sendMessageButton = GetSendMessageButton();

        if (messageTextBox is null)
        {
            _logger.Warning("Cannot find the Message box.");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Clipboard.SetText(message);
                System.Windows.Clipboard.Flush();
            });
            SelectAll();
            Paste();
        }
        else
        {
            messageTextBox.Focus();
            messageTextBox.Click(true);
            SelectAll();
            messageTextBox.Enter(message);
        }

        await Task.Delay(TimeSpan.FromSeconds(2));

        if (sendMessageButton is null)
        {
            _logger.Warning("Cannot find the `Send` button.");
            Send();
        }
        else
        {
            sendMessageButton.Click(true);
            messageTextBox?.Click(true);
        }

        return true;
    }

    public async Task<bool> SendFileMessageAsync(string path)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.Information("Set file to Clipboard.");
            System.Windows.Clipboard.SetFileDropList(new StringCollection { path });
        });

        var messageTextBox = GetMessageTextBox();
        var sendMessageButton = GetSendMessageButton();

        if (messageTextBox is null)
        {
            _logger.Warning("Cannot find the Message box.");
        }
        else
        {
            messageTextBox.Focus();
            messageTextBox.Click(true);
        }

        SelectAll();
        Paste();

        await Task.Delay(TimeSpan.FromSeconds(2));
        if (sendMessageButton is null)
        {
            _logger.Warning("Cannot find the `Send` button.");
            Send();
        }
        else
        {
            sendMessageButton.Click(true);
            messageTextBox?.Click(true);
        }

        return true;
    }

    public static void SelectAll()
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
    }

    public static void Paste()
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V);
    }

    public static void Send()
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.KEY_S);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _automation?.Dispose();
    }
}
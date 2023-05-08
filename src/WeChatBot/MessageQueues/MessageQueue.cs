using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WeChatBot.Models.Messages;
using WeChatBot.Services;

namespace WeChatBot.MessageQueues;

/// <summary>
/// A message queue for Producer Consumer Problem.
/// </summary>
public class MessageQueue : IDisposable
{
    /// <summary>
    /// The queue of messages waiting to be sent to Wechat.
    /// </summary>
    private readonly ConcurrentQueue<Message> _queue = new();

    private readonly AutomateService _automateService;
    private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(initialState: false);

    private bool _running;

    public MessageQueue(AutomateService automateService)
    {
        _automateService = automateService;
    }

    public void Enqueue(Message message)
    {
        _queue.Enqueue(message);
        _eventWaitHandle.Set();
    }

    private async Task WorkAsync()
    {
        while (_running)
        {
            if (_queue.IsEmpty)
            {
                _eventWaitHandle.WaitOne();
            }
            else
            {
                if (_queue.TryDequeue(out var message))
                {
                    switch (message)
                    {
                        case FileMessage fileMessage:
                            await _automateService.SendFileMessageAsync(fileMessage.Path);
                            break;
                        case TextMessage textMessage:
                            await _automateService.SendTextMessageAsync(textMessage.Text);
                            break;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _running = false;
        _eventWaitHandle?.Dispose();
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        Task.Run(WorkAsync);
    }
}
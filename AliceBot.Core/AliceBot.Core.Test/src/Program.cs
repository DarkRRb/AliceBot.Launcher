using AliceBot.Core.Actions;
using AliceBot.Core.Actions.Results;
using AliceBot.Core.Events;
using AliceBot.Core.Events.Data;
using AliceBot.Core.Handlers;
using AliceBot.Core.Loggers;
using AliceBot.Core.Messages;
using AliceBot.Core.Protocols;

namespace AliceBot.Core.Test;

internal class Program {
    private static async Task Main(string[] args) {
        Alice alice = new((tag) => new SimpleLogger(tag), "config.jsonc");

        CancellationTokenSource cts = new();
        void handler(object? _, ConsoleCancelEventArgs @event) {
            cts.Cancel();
            @event.Cancel = true;
        }
        Console.CancelKeyPress += handler;

        await alice.RegisterProtocolAsync("simple", (alice) => new SimpleProtocol(alice), cts.Token);

        await alice.StartAsync(cts.Token);

        await alice.RegisterHandlerAsync("simple", (alice) => new SimpleHandler(alice), cts.Token);

        Console.CancelKeyPress -= handler;
        TaskCompletionSource tcs = new();
        Console.CancelKeyPress += (_, @event) => {
            tcs.SetResult();
            @event.Cancel = true;
        };
        await tcs.Task;
        Console.CancelKeyPress += handler;

        await alice.StopAsync(cts.Token);
    }
}

internal class SimpleLogger(string tag) : ILogger {
    private readonly string _tag = tag;

    public void Log(ILogger.Level level, string message) {
        Console.WriteLine($"[{_tag}] [{level}] {message}");
    }
}

internal class SimpleProtocol(Alice alice) : IProtocol, IProtocolEvents, IActions {
    private readonly ILogger _logger = alice.GetLogger("SimpleProtocol");

    private CancellationTokenSource _cts = new();

    public IProtocolEvents Events => this;

    public IActions Actions => this;

    public event Action<IProtocol, GroupMessageData>? OnGroupMessage;

    public event Action<IProtocol, PrivateMessageData>? OnPrivateMessage;

    public async Task ScheduledGroupMessageEventsAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            await Task.Delay(1500, token);
            OnGroupMessage?.Invoke(this, new(new(
                "messageId",
                DateTimeOffset.Now,
                "groupId",
                "userId",
                new MessageContent.Builder().Text("Group Message Text!").Build()
            )));
        }
    }

    public async Task ScheduledPrivateMessageEventsAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            await Task.Delay(1000, token);
            OnPrivateMessage?.Invoke(this, new(new(
                "messageId",
                DateTimeOffset.Now,
                "userId",
                new MessageContent.Builder().Text("Private Message Text!").Build()
            )));
        }
    }


    public Task<GetSelfInfoResult> GetSelfInfo(CancellationToken token) {
        _logger.Log(ILogger.Level.Info, "Get self info.");
        return Task.FromResult(new GetSelfInfoResult("userId", "nickname"));
    }

    public Task RecallMessageAsync(string messageId, CancellationToken token) {
        _logger.Log(ILogger.Level.Info, $"Recall message: {messageId}");
        return Task.CompletedTask;
    }

    public Task<SendGroupMessageResult> SendGroupMessageAsync(string groupId, MessageContent message, CancellationToken token) {
        _logger.Log(ILogger.Level.Info, $"Send group message: {groupId} {message}");
        return Task.FromResult(new SendGroupMessageResult("messageId"));
    }

    public Task<SendPrivateMessageResult> SendPrivateMessageAsync(string userId, MessageContent message, CancellationToken token) {
        _logger.Log(ILogger.Level.Info, $"Send private message: {userId} {message}");
        return Task.FromResult(new SendPrivateMessageResult("messageId"));
    }

    public Task StartAsync(CancellationToken token) {
        _ = ScheduledGroupMessageEventsAsync(_cts.Token);
        _ = ScheduledPrivateMessageEventsAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        _cts.Cancel();
        _cts = new();
        return Task.CompletedTask;
    }
}

internal class SimpleHandler(Alice alice) : IHandler {
    private readonly Alice _alice = alice;

    private readonly ILogger _logger = alice.GetLogger("SimpleHandler");

    private CancellationTokenSource _cts = new();

    public void HandleAliceStarted() {
        _logger.Log(ILogger.Level.Info, "Alice is started.");
    }

    public void HandleAliceStopped() {
        _logger.Log(ILogger.Level.Info, "Alice is stopped.");
    }

    public async void HandleGroupMessage(IProtocol protocol, GroupMessageData data) {
        _logger.Log(ILogger.Level.Info, $"Handle group message: GroupMessageData(Message(MessageId: {data.Message.MessageId}, Time: {data.Message.Time}, GroupId: {data.Message.GroupId}, UserId: {data.Message.UserId}, Content: {data.Message.Content}))");
        await protocol.Actions.SendGroupMessageAsync(data.Message.GroupId, new MessageContent.Builder().Text("Reply Group Message Text!").Build(), _cts.Token);
        await protocol.Actions.RecallMessageAsync(data.Message.MessageId, _cts.Token);
    }

    public async void HandlePrivateMessage(IProtocol protocol, PrivateMessageData data) {
        _logger.Log(ILogger.Level.Info, $"Handle private message: PrivateMessageData(Message(MessageId: {data.Message.MessageId}, Time: {data.Message.Time}, UserId: {data.Message.UserId}, Content: {data.Message.Content}))");
        await protocol.Actions.SendPrivateMessageAsync(data.Message.UserId, new MessageContent.Builder().Text("Reply Private Message Text!").Build(), _cts.Token);
        await protocol.Actions.RecallMessageAsync(data.Message.MessageId, _cts.Token);
    }

    public Task StartAsync(CancellationToken token) {
        _alice.Events.OnAliceStarted += HandleAliceStarted;
        _alice.Events.OnAliceStopped += HandleAliceStopped;
        _alice.Events.OnGroupMessage += HandleGroupMessage;
        _alice.Events.OnPrivateMessage += HandlePrivateMessage;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        _alice.Events.OnAliceStarted -= HandleAliceStarted;
        _alice.Events.OnAliceStopped -= HandleAliceStopped;
        _alice.Events.OnGroupMessage -= HandleGroupMessage;
        _alice.Events.OnPrivateMessage -= HandlePrivateMessage;

        _cts.Cancel();
        _cts = new();

        return Task.CompletedTask;
    }
}
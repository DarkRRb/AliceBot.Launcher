using System;
using AliceBot.Core.Events.Data;
using AliceBot.Core.Protocols;

namespace AliceBot.Core.Events;

public class AliceEvents : IProtocolEvents {
    public event Action? OnAliceStarted;

    public event Action? OnAliceStopped;

    public event Action<IProtocol, PrivateMessageData>? OnPrivateMessage;

    public event Action<IProtocol, GroupMessageData>? OnGroupMessage;

    internal void EmitAliceStarted() {
        OnAliceStarted?.Invoke();
    }

    internal void EmitAliceStopped() {
        OnAliceStopped?.Invoke();
    }

    private void EmitPrivateMessage(IProtocol protocol, PrivateMessageData data) {
        OnPrivateMessage?.Invoke(protocol, data);
    }

    private void EmitGroupMessage(IProtocol protocol, GroupMessageData data) {
        OnGroupMessage?.Invoke(protocol, data);
    }

    internal void Aggregate(IProtocolEvents events) {
        events.OnPrivateMessage += EmitPrivateMessage;
        events.OnGroupMessage += EmitGroupMessage;
    }

    internal void Disaggregate(IProtocolEvents events) {
        events.OnPrivateMessage -= EmitPrivateMessage;
        events.OnGroupMessage -= EmitGroupMessage;
    }
}
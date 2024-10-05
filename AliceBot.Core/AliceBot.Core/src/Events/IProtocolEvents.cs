using System;
using AliceBot.Core.Events.Data;
using AliceBot.Core.Protocols;

namespace AliceBot.Core.Events;

public interface IProtocolEvents {
    public event Action<IProtocol, PrivateMessageData>? OnPrivateMessage;

    public event Action<IProtocol, GroupMessageData>? OnGroupMessage;
}
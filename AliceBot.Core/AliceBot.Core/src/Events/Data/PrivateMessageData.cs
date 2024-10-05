using AliceBot.Core.Messages;

namespace AliceBot.Core.Events.Data;

public class PrivateMessageData(PrivateMessage message) {
    public PrivateMessage Message { get; } = message;
}
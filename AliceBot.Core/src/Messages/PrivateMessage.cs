using System;

namespace AliceBot.Core.Messages;

public class PrivateMessage(string messageId, DateTimeOffset time, string userId, MessageContent content) {
    public string MessageId { get; } = messageId;

    public DateTimeOffset Time { get; } = time;

    public string UserId { get; } = userId;

    public MessageContent Content { get; } = content;
}
using System;

namespace AliceBot.Core.Messages;

public class GroupMessage(string messageId, DateTimeOffset time, string groupId, string userId, MessageContent content) {
    public string MessageId { get; } = messageId;

    public DateTimeOffset Time { get; } = time;

    public string GroupId { get; } = groupId;

    public string UserId { get; } = userId;
    
    public MessageContent Content { get; } = content;
}
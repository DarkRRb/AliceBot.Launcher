using AliceBot.Core.Messages;

namespace AliceBot.Core.Events.Data;

public class GroupMessageData(GroupMessage message) {
    public GroupMessage Message { get; } = message;
}
namespace AliceBot.Core.Actions.Results;

public class SendGroupMessageResult(string messageId) {
    public string MessageId { get; } = messageId;
}
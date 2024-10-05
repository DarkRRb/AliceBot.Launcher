namespace AliceBot.Core.Actions.Results;

public class SendPrivateMessageResult(string messageId) {
    public string MessageId { get; } = messageId;
}
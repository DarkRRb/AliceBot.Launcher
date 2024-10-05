namespace AliceBot.Core.Messages.Segments;

public class ReplySegment(string messageId) : ISegment {
    public string MessageId { get; } = messageId;
}
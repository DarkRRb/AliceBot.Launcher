namespace AliceBot.Core.Messages.Segments;

public class EmojiSegment(string emojiId) : ISegment {
    public string EmojiId { get; } = emojiId;
}
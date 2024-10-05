namespace AliceBot.Core.Messages.Segments;

public class TextSegment(string text) : ISegment {
    public string Text { get; } = text;
}
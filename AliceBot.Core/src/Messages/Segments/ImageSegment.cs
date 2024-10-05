namespace AliceBot.Core.Messages.Segments;

public class ImageSegment(string url) : ISegment {
    public string Url { get; } = url;
}
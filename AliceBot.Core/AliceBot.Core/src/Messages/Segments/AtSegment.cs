namespace AliceBot.Core.Messages.Segments;

public class AtSegment(string userId) : ISegment {
    public string UserId { get; } = userId;
}
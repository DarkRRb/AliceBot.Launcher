namespace AliceBot.Core.Actions.Results;

public class GetSelfInfoResult(string userId, string username) {
    public string UserId { get; } = userId;
    
    public string Username { get; } = username;
}
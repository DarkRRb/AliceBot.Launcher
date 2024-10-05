using AliceBot.Core.Loggers;

namespace AliceBot.Launcher.Loggers;

public class AliceLoggerFactory {
    public ILogger.Level MinLevel { get; set; } = ILogger.Level.Info;

    public AliceLogger Create(string tag) {
        return new(this, tag);
    }
}
namespace AliceBot.Core.Loggers;

public interface ILogger {
    public void Log(Level level, string message);

    public void Trace(string message) {
        Log(Level.Trace, message);
    }

    public void Info(string message) {
        Log(Level.Info, message);
    }

    public void Warn(string message) {
        Log(Level.Warn, message);
    }

    public void Error(string message) {
        Log(Level.Error, message);
    }

    public enum Level {
        Trace,
        Info,
        Warn,
        Error,
    }
}
using AliceBot.Core.Loggers;
using AliceBot.Launcher.Loggers;

namespace AliceBot.Launcher;

public class AliceLogger(AliceLoggerFactory factory, string tag) : ILogger {
    private readonly string _tag = tag;

    private readonly AliceLoggerFactory _factory = factory;

    public void Log(ILogger.Level level, string message) {
        if (_factory.MinLevel > level) {
            return;
        }

        switch (level) {
            case ILogger.Level.Trace: {
                Console.ForegroundColor = ConsoleColor.White;
                break;
            }
            case ILogger.Level.Info: {
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            }
            case ILogger.Level.Warn: {
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            }
            case ILogger.Level.Error: {
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            }
        }
        Console.WriteLine($"[{level}] [{_tag}] {message}");
        Console.ResetColor();
    }
}
using System.Text.Json.Serialization;
using AliceBot.Core.Loggers;

namespace AliceBot.Launcher.Configs;

public class LoggerConfig {
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ILogger.Level MinLevel { get; set; } = ILogger.Level.Info;
}
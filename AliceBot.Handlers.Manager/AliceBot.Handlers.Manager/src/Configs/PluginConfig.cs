using System.Text.Json.Serialization;

namespace AliceBot.Handlers.Manager.Configs;

public class PluginConfig {
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PluginType Type { get; set; }

    public string? Key { get; set; }

    public required string DllPath { get; set; }

    public required string TypeFullName { get; set; }

    public required string MethodName { get; set; }

    public enum PluginType {
        Protocol,
        Handler
    }
}
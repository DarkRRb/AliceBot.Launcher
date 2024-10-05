using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AliceBot.Core.Configs;

public class ConfigManager(string configPath) {
    private readonly string _configPath = configPath;

    private static readonly JsonDocumentOptions _documentOptions = new() {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions _options = new() {
        WriteIndented = true,
        IndentSize = 4
    };

    // Config
    private readonly JsonNode _config = JsonNode.Parse(File.ReadAllText(configPath), null, _documentOptions)
        ?? throw new Exception("Config file is empty.");

    public T Get<T>(string key) {
        return _config[key].Deserialize<T>() ?? throw new Exception($"Config key '{key}' is not found.");
    }

    public void Save<T>(string key, T value) {
        _config[key] = JsonSerializer.SerializeToNode(value);
        File.WriteAllText(_configPath, _config.ToJsonString(_options));
    }
}
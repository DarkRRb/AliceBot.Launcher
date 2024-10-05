using System.Collections.Generic;

namespace AliceBot.Handlers.Manager.Configs;

public class RootConfig {
    public required List<PluginConfig> Defaults { get; set; }
}
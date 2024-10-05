using System.CommandLine;

namespace AliceBot.Handlers.Manager.Utilities;

public static class CommandUtility {
    public static Option<string> KeyOption { get; } = new(
        "--key",
        "The key of the plugin, default is the path of the dll"
    );

    public static Option<string> DllOption { get; } = new("--dll", "The path of the dll");

    public static Option<string> TypeOption { get; } = new("--type", "The full name of the type");

    public static Option<string> MethodOption { get; } = new("--method", "The name of the method");

    public static Option<bool> DefaultOption { get; } = new("--default", "Add or remove plugin to default");

    public static Command LoadProtocolCommand { get; } = new("protocol", "Load a protocol") {
        KeyOption,
        DllOption,
        TypeOption,
        MethodOption,
        DefaultOption
    };

    public static Command LoadHandlerCommand { get; } = new("handler", "Load a handler") {
        KeyOption,
        DllOption,
        TypeOption,
        MethodOption,
        DefaultOption
    };

    public static Command UnloadProtocolCommand { get; } = new("protocol", "Unload a protocol") {
        KeyOption,
        DefaultOption
    };

    public static Command UnloadHandlerCommand { get; } = new("handler", "Unload a handler") {
        KeyOption,
        DefaultOption
    };

    public static Command ListContextCommand { get; } = new("context", "List all contexts");

    public static Command ListReferenceCommand { get; } = new("reference", "List all references");

    public static Command LoadCommand { get; } = new("load", "Load a plugin") {
        LoadProtocolCommand,
        LoadHandlerCommand
    };

    public static Command UnloadCommand { get; } = new("unload", "Unload a plugin") {
        UnloadProtocolCommand,
        UnloadHandlerCommand
    };

    public static Command ListCommand { get; } = new("list", "List something") {
        ListContextCommand,
        ListReferenceCommand
    };

    public static Command FlushCommand { get; } = new("flush", "Flush all references");

    public static Command ManagerCommand { get; } = new("#manager", "Manager command") {
        LoadCommand,
        UnloadCommand,
        ListCommand,
        FlushCommand
    };
}
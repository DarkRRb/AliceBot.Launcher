using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AliceBot.Core;
using AliceBot.Core.Actions.Results;
using AliceBot.Core.Events.Data;
using AliceBot.Core.Handlers;
using AliceBot.Core.Loggers;
using AliceBot.Core.Messages;
using AliceBot.Core.Messages.Segments;
using AliceBot.Core.Protocols;
using AliceBot.Handlers.Manager.Configs;
using AliceBot.Handlers.Manager.Plugins;
using AliceBot.Handlers.Manager.Utilities;
using static AliceBot.Core.Alice;

namespace AliceBot.Handlers.Manager;

public partial class ManagerHandler(Alice alice) : IHandler {
    private const string CONFIG_KEY = "Manager";

    private readonly Alice _alice = alice;

    private readonly ILogger _logger = alice.GetLogger(nameof(ManagerHandler));

    private readonly RootConfig _config = alice.GetConfig<RootConfig>(CONFIG_KEY);

    private CancellationTokenSource _cts = new();

    private readonly Dictionary<string, PluginLoadContext> _contexts = [];

    private readonly Dictionary<string, WeakReference> _references = [];

    public static ManagerHandler Create(Alice alice) {
        return new(alice);
    }

    private async Task LoadProtocolAsync(string key, string dllPath, string typeFullName, string methodName, CancellationToken token) {
        if (_contexts.ContainsKey(dllPath)) throw new Exception($"Plugin {dllPath} already loaded.");

        PluginLoadContext? context = null;
        try {
            context = new(dllPath);
            Func<Alice, IProtocol> factory = context.LoadFoctory<Func<Alice, IProtocol>>(typeFullName, methodName);
            await _alice.RegisterProtocolAsync(key, factory, token);
            _contexts[key] = context;
        } catch {
            _references.Add(key, new(context));
            context?.Unload();
            _contexts.Remove(dllPath);
            throw;
        }
    }

    private async Task LoadHandlerAsync(string key, string dllPath, string typeFullName, string methodName, CancellationToken token) {
        if (_contexts.ContainsKey(dllPath)) throw new Exception($"Plugin {dllPath} already loaded.");

        PluginLoadContext? context = null;
        try {
            context = new(dllPath);
            Func<Alice, IHandler> factory = context.LoadFoctory<Func<Alice, IHandler>>(typeFullName, methodName);
            await _alice.RegisterHandlerAsync(key, factory, token);
            _contexts[key] = context;
        } catch {
            _references.Add(key, new(context));
            context?.Unload();
            _contexts.Remove(dllPath);
            throw;
        }
    }

    private async Task LoadPluginAsync(PluginConfig config, CancellationToken token) {
        switch (config.Type) {
            case PluginConfig.PluginType.Protocol: {
                await LoadProtocolAsync(
                    config.Key ?? config.DllPath,
                    config.DllPath,
                    config.TypeFullName,
                    config.MethodName,
                    token
                );
                break;
            }
            case PluginConfig.PluginType.Handler: {
                await LoadHandlerAsync(
                    config.Key ?? config.DllPath,
                    config.DllPath,
                    config.TypeFullName,
                    config.MethodName,
                    token
                );
                break;
            }
            default: {
                throw new Exception($"Unknown plugin type {config.Type}.");
            }
        }
    }

    private async void AliceStartedHandler() {
        try {
            foreach ((string key, WeakReference reference) in _references) {
                if (!reference.IsAlive) _references.Remove(key);
            }

            foreach (var plugin in _config.Defaults) {
                await LoadPluginAsync(plugin, _cts.Token);
            }
        } catch (Exception e) {
            _logger.Error(e.ToString());
        }
    }

    private async Task LoadProtocolAsync(CommandResult result, CancellationToken token) {
        string? key = result.GetValueForOption(CommandUtility.KeyOption);
        string dllPath = result.GetValueForOption(CommandUtility.DllOption)
            ?? throw new Exception("--dll(dll path) is required.");
        string typeFullName = result.GetValueForOption(CommandUtility.TypeOption)
            ?? throw new Exception("--type(type full name) is required.");
        string methodName = result.GetValueForOption(CommandUtility.MethodOption)
            ?? throw new Exception("--method(method name) is required.");

        await LoadProtocolAsync(key ?? dllPath, dllPath, typeFullName, methodName, token);

        if (result.GetValueForOption(CommandUtility.DefaultOption)) {
            if (!_config.Defaults.Any(plugin => (key != null && plugin.Key == key) || plugin.DllPath == dllPath)) {
                _config.Defaults.Add(new() {
                    Type = PluginConfig.PluginType.Protocol,
                    Key = key,
                    DllPath = dllPath,
                    TypeFullName = typeFullName,
                    MethodName = methodName
                });
                _alice.SaveConfig(CONFIG_KEY, _config);
            }
        }
    }

    private async Task LoadHandlerAsync(CommandResult result, CancellationToken token) {
        string? key = result.GetValueForOption(CommandUtility.KeyOption);
        string dllPath = result.GetValueForOption(CommandUtility.DllOption)
            ?? throw new Exception("--dll(dll path) is required.");
        string typeFullName = result.GetValueForOption(CommandUtility.TypeOption)
            ?? throw new Exception("--type(type full name) is required.");
        string methodName = result.GetValueForOption(CommandUtility.MethodOption)
            ?? throw new Exception("--method(method name) is required.");

        await LoadHandlerAsync(key ?? dllPath, dllPath, typeFullName, methodName, token);

        if (result.GetValueForOption(CommandUtility.DefaultOption)) {
            if (!_config.Defaults.Any(plugin => (key != null && plugin.Key == key) || plugin.DllPath == dllPath)) {
                _config.Defaults.Add(new() {
                    Type = PluginConfig.PluginType.Handler,
                    Key = key,
                    DllPath = dllPath,
                    TypeFullName = typeFullName,
                    MethodName = methodName
                });
                _alice.SaveConfig(CONFIG_KEY, _config);
            }
        }
    }

    public async Task UnloadProtocolAsync(CommandResult result, CancellationToken token) {
        string key = result.GetValueForOption(CommandUtility.KeyOption)
            ?? throw new Exception("--key(key) is required.");

        await _alice.UnregisterProtocolAsync(key, token);

        if (!_contexts.Remove(key, out PluginLoadContext? context)) {
            throw new Exception($"Protocol {key} unmanaged (unload success).");
        }

        _references.Add(key, new(context));
        context.Unload();
    }

    public async Task UnloadHandlerAsync(CommandResult result, CancellationToken token) {
        string key = result.GetValueForOption(CommandUtility.KeyOption)
            ?? throw new Exception("--key(key) is required.");

        await _alice.UnregisterHandlerAsync(key, token);

        if (!_contexts.Remove(key, out PluginLoadContext? context)) {
            throw new Exception($"Handler {key} unmanaged (unload success).");
        }

        _references.Add(key, new(context));
        context.Unload();
    }

    private async void GroupMessageHandler(IProtocol protocol, GroupMessageData data) {
        try {
            foreach ((string key, WeakReference reference) in _references) {
                if (!reference.IsAlive) _references.Remove(key);
            }

            GroupMessage message = data.Message;

            if (message.Content.Any(segment => segment is not TextSegment)) return;

            string command = message.Content.Aggregate(
                new StringBuilder(),
                (builder, segment) => builder.Append(((TextSegment)segment).Text),
                builder => builder.ToString()
            );

            ParseResult result = CommandUtility.ManagerCommand.Parse(command);
            if (result.Errors.Count > 0) return;

            if (result.CommandResult.Command == CommandUtility.LoadProtocolCommand) {
                await LoadProtocolAsync(result.CommandResult, _cts.Token);
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Text("Protocol loaded.").Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.LoadHandlerCommand) {
                await LoadHandlerAsync(result.CommandResult, _cts.Token);
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Text("Handler loaded.").Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.UnloadProtocolCommand) {
                await UnloadProtocolAsync(result.CommandResult, _cts.Token);
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Text("Protocol unloaded.").Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.UnloadHandlerCommand) {
                await UnloadHandlerAsync(result.CommandResult, _cts.Token);
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Text("Handler unloaded.").Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.ListContextCommand) {
                GetSelfInfoResult selfInfo = await protocol.Actions.GetSelfInfo(_cts.Token);

                ForwardSegment.ForwardMessage first = new ForwardSegment.ForwardMessage.Builder()
                    .SetUserId(selfInfo.UserId)
                    .Build(new MessageContent.Builder().Text("Contexts:").Build());

                ForwardSegment.ForwardMessage[] messages = _contexts.Keys
                    .Select(key => new ForwardSegment.ForwardMessage.Builder()
                        .SetUserId(selfInfo.UserId)
                        .Build(new MessageContent.Builder().Text(key).Build())
                    )
                    .ToArray();
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Forward([first, .. messages]).Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.ListReferenceCommand) {
                GetSelfInfoResult selfInfo = await protocol.Actions.GetSelfInfo(_cts.Token);

                ForwardSegment.ForwardMessage first = new ForwardSegment.ForwardMessage.Builder()
                    .SetUserId(selfInfo.UserId)
                    .Build(new MessageContent.Builder().Text("References:").Build());

                ForwardSegment.ForwardMessage[] messages = _references.Keys
                    .Select(key => new ForwardSegment.ForwardMessage.Builder()
                        .SetUserId(selfInfo.UserId)
                        .Build(new MessageContent.Builder().Text(key).Build())
                    )
                    .ToArray();
                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Forward([first, .. messages]).Build(),
                    _cts.Token
                );
            } else if (result.CommandResult.Command == CommandUtility.FlushCommand) {
                for (int i = 0; i < 10; i++) {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                foreach ((string key, WeakReference reference) in _references) {
                    if (!reference.IsAlive) _references.Remove(key);
                }

                await protocol.Actions.SendGroupMessageAsync(
                    message.GroupId,
                    new MessageContent.Builder().Text("Flushed.").Build(),
                    _cts.Token
                );
            }
        } catch (Exception e) {
            string errorText = e.ToString();

            _logger.Error(errorText);

            await protocol.Actions.SendGroupMessageAsync(
                data.Message.GroupId,
                new MessageContent.Builder().Text(errorText).Build(),
                _cts.Token
            );
        }
    }

    public Task StartAsync(CancellationToken token) {
        _alice.Events.OnAliceStarted += AliceStartedHandler;
        if (_alice.Status == AliceStatus.Started) {
            AliceStartedHandler();
        }
        _alice.Events.OnGroupMessage += GroupMessageHandler;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        _cts.Cancel();
        _cts = new();

        _alice.Events.OnAliceStarted -= AliceStartedHandler;
        _alice.Events.OnGroupMessage -= GroupMessageHandler;

        return Task.CompletedTask;
    }
}
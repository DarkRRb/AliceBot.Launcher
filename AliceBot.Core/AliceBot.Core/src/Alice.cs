using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AliceBot.Core.Configs;
using AliceBot.Core.Events;
using AliceBot.Core.Handlers;
using AliceBot.Core.Loggers;
using AliceBot.Core.Protocols;
using AliceBot.Core.Utilities.Locks;

namespace AliceBot.Core;

public class Alice(Func<string, ILogger> loggerFactory, string configPath) {
    private readonly Func<string, ILogger> _loggerFactory = loggerFactory;

    private readonly ILogger _logger = loggerFactory("Alice");

    private readonly ConfigManager _config = new(configPath);

    // Alice Lock
    private readonly AsyncReaderWriterLock _lock = new();

    // Alice State
    private AliceStatus _status = AliceStatus.Stopped;

    public AliceStatus Status => _status;

    // Alice Event
    public AliceEvents Events { get; } = new();

    // Key and ProtocolFactorys
    private readonly Dictionary<string, Func<Alice, IProtocol>> _protocolFactorys = [];

    // Key and Protocol
    private readonly Dictionary<string, IProtocol> _protocols = [];

    // Key and HandlerFactorys
    private readonly Dictionary<string, Func<Alice, IHandler>> _handlerFactorys = [];

    // Key and Handler
    private readonly Dictionary<string, IHandler> _handlers = [];

    // Get Logger
    public ILogger GetLogger(string key) {
        return _loggerFactory(key);
    }

    // Get Config
    public T GetConfig<T>(string key) {
        return _config.Get<T>(key);
    }

    // Save Config
    public void SaveConfig<T>(string key, T value) {
        _config.Save(key, value);
    }

    // Protocol Register
    public async Task RegisterProtocolAsync(string key, Func<Alice, IProtocol> protocolFactory, CancellationToken token) {
        _logger.Info($"Registering protocol '{key}'.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (_protocols.ContainsKey(key)) {
                throw new Exception($"Protocol '{key}' is already registered.");
            }

            _protocolFactorys[key] = protocolFactory;

            if (_status == AliceStatus.Started) {
                _protocols[key] = protocolFactory(this);
                await _protocols[key].StartAsync(token);
                Events.Aggregate(_protocols[key].Events);
            }
        } catch {
            _protocolFactorys.Remove(key);
            _protocols.Remove(key);
            throw;
        } finally { _lock.ExitWriteLock(); }

        _logger.Info($"Protocol '{key}' is registered.");
    }

    // Protocol Unregister
    public async Task UnregisterProtocolAsync(string key, CancellationToken token) {
        _logger.Info($"Unregistering protocol '{key}'.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (!_protocols.TryGetValue(key, out IProtocol? protocol)) {
                throw new Exception($"Protocol '{key}' is not registered.");
            }

            if (_status == AliceStatus.Started) {
                Events.Disaggregate(protocol.Events);
                await protocol.StopAsync(token);
                _protocols.Remove(key);
            }

            _protocolFactorys.Remove(key);
        } finally { _lock.ExitWriteLock(); }

        _logger.Info($"Protocol '{key}' is unregistered.");
    }

    // Get Protocol
    public async Task<IProtocol> GetProtocolAsync(string key, CancellationToken token) {
        await _lock.EnterReadLockAsync(token);
        try {
            if (!_protocols.TryGetValue(key, out IProtocol? protocol)) {
                throw new Exception($"Protocol '{key}' is not registered.");
            }

            return protocol;
        } finally { _lock.ExitReadLock(); }
    }

    // Handler Register
    public async Task RegisterHandlerAsync(string key, Func<Alice, IHandler> handlerFactory, CancellationToken token) {
        _logger.Info($"Registering handler '{key}'.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (_handlers.ContainsKey(key)) {
                throw new Exception($"Handler '{key}' is already registered.");
            }

            _handlerFactorys[key] = handlerFactory;

            if (_status == AliceStatus.Started) {
                _handlers[key] = handlerFactory(this);
                await _handlers[key].StartAsync(token);
            }
        } catch {
            _handlerFactorys.Remove(key);
            _handlers.Remove(key);
            throw;
        } finally { _lock.ExitWriteLock(); }

        _logger.Info($"Handler '{key}' is registered.");
    }

    // Handler Unregister
    public async Task UnregisterHandlerAsync(string key, CancellationToken token) {
        _logger.Info($"Unregistering handler '{key}'.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (!_handlers.TryGetValue(key, out IHandler? handler)) {
                throw new Exception($"Handler '{key}' is not registered.");
            }

            if (_status == AliceStatus.Started) {
                await handler.StopAsync(token);
                _handlers.Remove(key);
            }

            _handlerFactorys.Remove(key);
        } finally { _lock.ExitWriteLock(); }

        _logger.Info($"Handler '{key}' is unregistered.");
    }

    // Get Handler
    public async Task<IHandler> GetHandlerAsync(string key, CancellationToken token) {
        await _lock.EnterReadLockAsync(token);
        try {
            if (!_handlers.TryGetValue(key, out IHandler? handler)) {
                throw new Exception($"Handler '{key}' is not registered.");
            }

            return handler;
        } finally { _lock.ExitReadLock(); }
    }

    // Start
    public async Task StartAsync(CancellationToken token) {
        _logger.Info("Alice is starting.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (_status != AliceStatus.Stopped) {
                throw new Exception("Alice is not stopped.");
            }
            _status = AliceStatus.Starting;

            foreach ((string key, Func<Alice, IProtocol> protocolFactory) in _protocolFactorys) {
                _protocols[key] = protocolFactory(this);
                await _protocols[key].StartAsync(token);
                Events.Aggregate(_protocols[key].Events);
            }

            foreach ((string key, Func<Alice, IHandler> handlerFactory) in _handlerFactorys) {
                _handlers[key] = handlerFactory(this);
                await _handlers[key].StartAsync(token);
            }

            _status = AliceStatus.Started;
        } finally { _lock.ExitWriteLock(); }
        Events.EmitAliceStarted();

        _logger.Info("Alice is started.");
    }

    // Stop
    public async Task StopAsync(CancellationToken token) {
        _logger.Info("Alice is stopping.");

        await _lock.EnterWriteLockAsync(token);
        try {
            if (_status != AliceStatus.Started) {
                throw new Exception("Alice is not started.");
            }
            _status = AliceStatus.Stopping;

            foreach (IProtocol protocol in _protocols.Values) {
                Events.Disaggregate(protocol.Events);
                await protocol.StopAsync(token);
            }
            _protocols.Clear();

            foreach (IHandler handler in _handlers.Values) {
                await handler.StopAsync(token);
            }
            _handlers.Clear();

            _status = AliceStatus.Stopped;
        } finally { _lock.ExitWriteLock(); }
        Events.EmitAliceStopped();

        _logger.Info("Alice is stopped.");
    }

    public enum AliceStatus {
        Starting,
        Started,
        Stopping,
        Stopped,
    }
}
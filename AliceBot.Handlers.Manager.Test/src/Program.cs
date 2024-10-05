using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AliceBot.Core;
using AliceBot.Core.Loggers;

namespace AliceBot.Handlers.Manager.Test;

public class Program {
    public static async Task Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;

        Alice alice = new(AliceLogger.Create, "config.jsonc");

        CancellationTokenSource cts = new();
        void handler(object? _, ConsoleCancelEventArgs @event) {
            cts.Cancel();
            @event.Cancel = true;
        }
        Console.CancelKeyPress += handler;

        await alice.RegisterHandlerAsync("Manager", ManagerHandler.Create, cts.Token);

        await alice.StartAsync(cts.Token);

        Console.CancelKeyPress -= handler;
        TaskCompletionSource tcs = new();
        Console.CancelKeyPress += (_, @event) => {
            tcs.SetResult();
            @event.Cancel = true;
        };
        await tcs.Task;
        Console.CancelKeyPress += handler;

        await alice.StopAsync(cts.Token);
    }
}

public class AliceLogger(string tag) : ILogger {
    private readonly string _tag = tag;

    public static AliceLogger Create(string tag) {
        return new(tag);
    }

    public void Log(ILogger.Level level, string message) {
        Console.WriteLine($"[{level}] [{_tag}] {message}");
    }
}
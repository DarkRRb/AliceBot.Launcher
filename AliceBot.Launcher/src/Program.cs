using System.Text;
using AliceBot.Core;
using AliceBot.Handlers.Manager;
using AliceBot.Launcher.Configs;
using AliceBot.Launcher.Loggers;

namespace AliceBot.Launcher;

public class Program {
    public static async Task Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;

        AliceLoggerFactory loggerFactory = new();
        Alice alice = new(loggerFactory.Create, "config.jsonc");
        loggerFactory.MinLevel = alice.GetConfig<LoggerConfig>("Logger").MinLevel;

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
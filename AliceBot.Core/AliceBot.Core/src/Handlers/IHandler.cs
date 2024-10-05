using System.Threading;
using System.Threading.Tasks;

namespace AliceBot.Core.Handlers;

public interface IHandler {
    public Task StartAsync(CancellationToken token);

    public Task StopAsync(CancellationToken token);
}
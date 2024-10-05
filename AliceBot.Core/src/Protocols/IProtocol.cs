using System.Threading;
using System.Threading.Tasks;
using AliceBot.Core.Actions;
using AliceBot.Core.Events;

namespace AliceBot.Core.Protocols;

public interface IProtocol {
    public IProtocolEvents Events { get; }

    public IActions Actions { get; }

    public Task StartAsync(CancellationToken token);

    public Task StopAsync(CancellationToken token);
}
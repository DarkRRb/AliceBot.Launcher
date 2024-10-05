using System.Threading;
using System.Threading.Tasks;

namespace AliceBot.Core.Utilities.Locks;

public class AsyncReaderWriterLock {
    private readonly SemaphoreSlim _mutex = new(1, 1);

    private uint _readCount = 0;

    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async Task EnterReadLockAsync(CancellationToken token) {
        await _mutex.WaitAsync(token);
        try {
            if (++_readCount == 1) await _writeLock.WaitAsync(token);
        } finally { _mutex.Release(); }
    }

    public void ExitReadLock() {
        _mutex.Wait();
        try {
            if (--_readCount == 0) _writeLock.Release();
        } finally { _mutex.Release(); }
    }

    public Task EnterWriteLockAsync(CancellationToken token) {
        return _writeLock.WaitAsync(token);
    }

    public void ExitWriteLock() {
        _writeLock.Release();
    }
}
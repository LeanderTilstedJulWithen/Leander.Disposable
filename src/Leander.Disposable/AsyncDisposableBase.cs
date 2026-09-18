namespace Leander.Disposable;

internal abstract class AsyncDisposableBase : IAsyncDisposableState
{
    private int _isDisposed;

    internal abstract ValueTask DisposeAsyncCore();

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            return ValueTask.CompletedTask;
        return DisposeAsyncCore();
    }

    public bool IsDisposed => _isDisposed != 0;
}

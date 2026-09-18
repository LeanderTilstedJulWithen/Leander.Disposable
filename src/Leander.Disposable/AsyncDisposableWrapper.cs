namespace Leander.Disposable;

internal sealed class AsyncDisposableWrapper(IAsyncDisposable disposable) : AsyncDisposableBase
{
    private readonly IAsyncDisposable _disposable = disposable;
    internal override ValueTask DisposeAsyncCore() => _disposable.DisposeAsync();
}

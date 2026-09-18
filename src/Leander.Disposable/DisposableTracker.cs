namespace Leander.Disposable;

internal sealed class DisposableTracker : DisposableBase, IDisposableTracker
{
    private readonly Stack<IDisposable> _disposables = [];
    internal override void DisposeCore()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }

    public void Track(IDisposable disposable)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _disposables.Push(disposable);
    }
}
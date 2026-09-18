namespace Leander.Disposable;

internal sealed class DisposableTracker : DisposableBase, IDisposableTracker
{
    private readonly Stack<IDisposable> _disposables = [];
    internal override void DisposeCore()
    {
        List<Exception>? exceptions = null;
        while (_disposables.TryPop(out var disposable))
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }
        if (exceptions is not null)
            throw new AggregateException(exceptions);
    }

    public void Track(IDisposable disposable)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _disposables.Push(disposable);
    }
}
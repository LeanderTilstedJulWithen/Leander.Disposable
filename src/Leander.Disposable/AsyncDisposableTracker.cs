namespace Leander.Disposable;

internal sealed class AsyncDisposableTracker : AsyncDisposableBase, IAsyncDisposableTracker
{
    private readonly Stack<IAsyncDisposable> _disposables = [];

    internal override async ValueTask DisposeAsyncCore()
    {
        List<Exception>? exceptions = null;
        while (_disposables.TryPop(out var disposable))
        {
            try
            {
                await disposable.DisposeAsync();
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }
        if (exceptions is not null)
            throw new AggregateException(exceptions);
    }

    private async ValueTask DisposeLifoOrder()
    {
        List<Exception>? exceptions = null;
        while (_disposables.TryPop(out var disposable))
        {
            try
            {
                await disposable.DisposeAsync();
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }
        if (exceptions is not null)
            throw new AggregateException(exceptions);
    }

    private async ValueTask DisposeParallel()
    {
        List<Exception>? exceptions = null;
        var tasks = new List<Task>();
        while (_disposables.TryPop(out var disposable))
        {
            try
            {
                var task = disposable.DisposeAsync().AsTask();
                if (task.IsCompletedSuccessfully)
                    continue;
                tasks.Add(task);
            }
            catch(Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            (exceptions ??= []).Add(ex);
        }

        if (exceptions is not null)
            throw new AggregateException(exceptions);
    }

    public void Track(IAsyncDisposable disposable)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _disposables.Push(disposable);
    }

    public void Track(IDisposable disposable)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _disposables.Push(new SyncAdapter(disposable));
    }

    private sealed class SyncAdapter(IDisposable disposable) : IAsyncDisposable
    {
        private readonly IDisposable _disposable = disposable;

        public ValueTask DisposeAsync()
        {
            _disposable.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}

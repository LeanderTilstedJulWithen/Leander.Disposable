namespace Leander.Disposable;

internal sealed class AsyncDisposableAction(Func<ValueTask> action) : AsyncDisposableBase
{
    private Func<ValueTask>? _action = action;

    internal override ValueTask DisposeAsyncCore()
    {
        var action = _action;
        _action = null;
        return action?.Invoke() ?? ValueTask.CompletedTask;
    }
}

namespace Leander.Disposable;

internal sealed class DisposableAction(Action action) : DisposableBase
{
    private Action? _action = action;

    internal override void DisposeCore()
    {
        _action?.Invoke();
        _action = null;
    }
}
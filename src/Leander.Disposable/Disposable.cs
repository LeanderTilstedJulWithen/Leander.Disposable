namespace Leander.Disposable;

public static class Disposable
{
    public static IDisposableState Create(Action action) => new DisposableAction(action);

    public static IDisposableState Wrap(IDisposable disposable) => new DisposableWrapper(disposable);
}
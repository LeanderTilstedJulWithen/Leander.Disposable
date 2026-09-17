namespace Leander.Disposable;

internal sealed class DisposableWrapper(IDisposable disposable) : DisposableBase
{
    private readonly IDisposable _disposable = disposable;
    internal override void DisposeCore() => _disposable.Dispose();
}
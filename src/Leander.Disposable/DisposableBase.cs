namespace Leander.Disposable;

internal abstract class DisposableBase : IDisposableState
{
    private bool _isDisposed;

    internal abstract void DisposeCore();

    #region IDisposableState
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        DisposeCore();
    }

    public bool IsDisposed => _isDisposed;
    #endregion
}
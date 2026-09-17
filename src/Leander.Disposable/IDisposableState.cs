namespace Leander.Disposable;

public interface IDisposableState : IDisposable
{
    public bool IsDisposed { get; }
}

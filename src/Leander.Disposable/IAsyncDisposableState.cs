namespace Leander.Disposable;

/// <summary>An <see cref="IAsyncDisposable"/> that exposes its disposal state.</summary>
public interface IAsyncDisposableState : IAsyncDisposable
{
    /// <summary>Gets a value indicating whether this instance has been disposed.</summary>
    bool IsDisposed { get; }
}

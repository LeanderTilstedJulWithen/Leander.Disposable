namespace Leander.Disposable;

/// <summary>An <see cref="IDisposable"/> that exposes its disposal state.</summary>
public interface IDisposableState : IDisposable
{
    /// <summary>Gets a value indicating whether this instance has been disposed.</summary>
    public bool IsDisposed { get; }
}

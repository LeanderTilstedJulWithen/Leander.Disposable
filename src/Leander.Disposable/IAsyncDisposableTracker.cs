namespace Leander.Disposable;

/// <summary>Collects disposable instances and disposes them all when the tracker itself is disposed.</summary>
public interface IAsyncDisposableTracker : IAsyncDisposableState
{
    /// <summary>Registers <paramref name="disposable"/> for disposal when this tracker is disposed.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker has already been disposed.</exception>
    void Track(IAsyncDisposable disposable);

    /// <summary>Registers <paramref name="disposable"/> for disposal when this tracker is disposed.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker has already been disposed.</exception>
    void Track(IDisposable disposable);
}

namespace Leander.Disposable;

/// <summary>Collects <see cref="IDisposable"/> instances and disposes them all when the tracker itself is disposed.</summary>
public interface IDisposableTracker : IDisposableState
{
    /// <summary>Registers <paramref name="disposable"/> for disposal when this tracker is disposed.</summary>
    /// <param name="disposable">The resource to track.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker has already been disposed.</exception>
    public void Track(IDisposable disposable);
}
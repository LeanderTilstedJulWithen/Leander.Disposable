namespace Leander.Disposable;

/// <summary>Factory for creating disposable utility objects.</summary>
public static class Disposable
{
    /// <summary>Creates a disposable that invokes <paramref name="action"/> exactly once on disposal.</summary>
    /// <param name="action">The cleanup action to run on disposal.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static IDisposableState Create(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new DisposableAction(action);
    }

    /// <summary>Creates a tracker that collects and disposes registered resources in reverse registration order.</summary>
    public static IDisposableTracker CreateTracker() => new DisposableTracker();

    /// <summary>Wraps an existing <see cref="IDisposable"/> to expose disposal state via <see cref="IDisposableState"/>.</summary>
    /// <param name="disposable">The disposable to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
    public static IDisposableState Wrap(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        return new DisposableWrapper(disposable);
    }
}
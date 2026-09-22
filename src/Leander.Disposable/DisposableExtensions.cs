namespace Leander.Disposable;

/// <summary>Extension methods for working with disposable state and trackers.</summary>
public static class DisposableExtensions
{
    /// <summary>Throws <see cref="ObjectDisposedException"/> if <paramref name="disposable"/> has already been disposed.</summary>
    /// <param name="disposable">The instance to check.</param>
    /// <param name="owner">The object whose type name should appear in the exception; defaults to <paramref name="disposable"/> itself.</param>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="disposable"/> is already disposed.</exception>
    public static void ThrowIfDisposed(this IDisposableState disposable, object? owner = null)
    {
        ObjectDisposedException.ThrowIf(disposable.IsDisposed, owner?.GetType() ?? disposable.GetType());
    }

    /// <summary>Tracks <paramref name="disposable"/> if it implements <see cref="IAsyncDisposable"/> or <see cref="IDisposable"/>; otherwise does nothing.</summary>
    /// <param name="tracker">The tracker to register with.</param>
    /// <param name="disposable">The candidate object, which may or may not be disposable.</param>
    public static void TrackIfDisposable(this IAsyncDisposableTracker tracker, object disposable)
    {
        if (disposable is IAsyncDisposable asyncDisposable)
            tracker.Track(asyncDisposable);
        else if (disposable is IDisposable syncDisposable)
            tracker.Track(syncDisposable);
    }

    /// <summary>Tracks <paramref name="disposable"/> if it implements <see cref="IDisposable"/>; otherwise does nothing.</summary>
    /// <param name="tracker">The tracker to register with.</param>
    /// <param name="disposable">The candidate object, which may or may not be disposable.</param>
    public static void TrackIfDisposable(this IDisposableTracker tracker, object disposable)
    {
        if (disposable is IDisposable syncDisposable)
            tracker.Track(syncDisposable);
    }

    /// <summary>Throws <see cref="ObjectDisposedException"/> if <paramref name="disposable"/> has already been disposed.</summary>
    /// <param name="disposable">The instance to check.</param>
    /// <param name="owner">The object whose type name should appear in the exception; defaults to <paramref name="disposable"/> itself.</param>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="disposable"/> is already disposed.</exception>
    public static void ThrowIfDisposed(this IAsyncDisposableState disposable, object? owner = null)
    {
        ObjectDisposedException.ThrowIf(disposable.IsDisposed, owner?.GetType() ?? disposable.GetType());
    }
}
namespace Leander.Disposable;

public static class DisposableExtensions
{
    public static void ThrowIfDisposed(this IDisposableState disposable, object? owner = null)
    {
        ObjectDisposedException.ThrowIf(disposable.IsDisposed, owner?.GetType() ?? disposable.GetType());
    }

    public static void TrackIfDisposable(this IAsyncDisposableTracker tracker, object disposable)
    {
        if (disposable is IAsyncDisposable asyncDisposable)
            tracker.Track(asyncDisposable);
        else if (disposable is IDisposable syncDisposable)
            tracker.Track(syncDisposable);
    }

    public static void TrackIfDisposable(this IDisposableTracker tracker, object disposable)
    {
        if (disposable is IDisposable syncDisposable)
            tracker.Track(syncDisposable);
    }

    public static void ThrowIfDisposed(this IAsyncDisposableState disposable, object? owner = null)
    {
        ObjectDisposedException.ThrowIf(disposable.IsDisposed, owner?.GetType() ?? disposable.GetType());
    }
}
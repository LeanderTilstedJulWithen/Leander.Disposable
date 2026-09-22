using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class DisposableExtensionsTests
{
    private sealed class Owner;

    [Fact]
    public void ThrowIfDisposed_Sync_DoesNotThrow_WhenNotDisposed()
    {
        var disposable = Disposable.Create(() => { });

        disposable.ThrowIfDisposed();
    }

    [Fact]
    public void ThrowIfDisposed_Sync_Throws_WhenDisposed()
    {
        var disposable = Disposable.Create(() => { });
        disposable.Dispose();

        Assert.Throws<ObjectDisposedException>(() => disposable.ThrowIfDisposed());
    }

    [Fact]
    public void ThrowIfDisposed_Sync_UsesOwnerTypeName_WhenProvided()
    {
        var disposable = Disposable.Create(() => { });
        disposable.Dispose();

        var ex = Assert.Throws<ObjectDisposedException>(() => disposable.ThrowIfDisposed(new Owner()));

        Assert.Equal(typeof(Owner).FullName, ex.ObjectName);
    }

    [Fact]
    public async Task ThrowIfDisposed_Async_DoesNotThrow_WhenNotDisposed()
    {
        var disposable = AsyncDisposable.Create(() => ValueTask.CompletedTask);

        disposable.ThrowIfDisposed();
        await disposable.DisposeAsync();
    }

    [Fact]
    public async Task ThrowIfDisposed_Async_Throws_WhenDisposed()
    {
        var disposable = AsyncDisposable.Create(() => ValueTask.CompletedTask);
        await disposable.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => disposable.ThrowIfDisposed());
    }

    [Fact]
    public void TrackIfDisposable_Sync_TracksIDisposable()
    {
        var log = new List<string>();
        var tracker = Disposable.CreateTracker();

        tracker.TrackIfDisposable(Disposable.Create(() => log.Add("tracked")));
        tracker.TrackIfDisposable("not disposable");

        tracker.Dispose();

        Assert.Equal(["tracked"], log);
    }

    [Fact]
    public async Task TrackIfDisposable_Async_PrefersIAsyncDisposable_OverIDisposable()
    {
        var log = new List<string>();
        var tracker = AsyncDisposable.CreateTracker();
        var dualDisposable = new DualDisposable(log);

        tracker.TrackIfDisposable(dualDisposable);
        await tracker.DisposeAsync();

        Assert.Equal(["async"], log);
    }

    private sealed class DualDisposable(List<string> log) : IDisposable, IAsyncDisposable
    {
        public void Dispose() => log.Add("sync");
        public ValueTask DisposeAsync()
        {
            log.Add("async");
            return ValueTask.CompletedTask;
        }
    }
}

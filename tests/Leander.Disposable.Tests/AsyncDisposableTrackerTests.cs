using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class AsyncDisposableTrackerTests
{
    private sealed class RecordingAsyncDisposable(List<string> log, string name) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            log.Add(name);
            return ValueTask.CompletedTask;
        }
    }

    private static IAsyncDisposable DelayedThrow(Exception exception) => AsyncDisposable.Create(async () =>
    {
        await Task.Yield(); // force completion onto the async path inside DisposeParallel
        throw exception;
    });

    [Fact]
    public async Task Track_AfterDispose_Throws()
    {
        var tracker = AsyncDisposable.CreateTracker();
        await tracker.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => tracker.Track(new RecordingAsyncDisposable([], "x")));
    }

    [Fact]
    public async Task DisposeAsync_Lifo_DisposesInReverseOrder()
    {
        var log = new List<string>();
        var tracker = AsyncDisposable.CreateTracker(DisposalOrder.Lifo);

        tracker.Track(new RecordingAsyncDisposable(log, "A"));
        tracker.Track(new RecordingAsyncDisposable(log, "B"));
        tracker.Track(new RecordingAsyncDisposable(log, "C"));

        await tracker.DisposeAsync();

        Assert.Equal(["C", "B", "A"], log);
    }

    [Fact]
    public async Task DisposeAsync_Lifo_CollectsAllExceptions()
    {
        var tracker = AsyncDisposable.CreateTracker(DisposalOrder.Lifo);

        tracker.Track(DelayedThrow(new InvalidOperationException("first")));
        tracker.Track(DelayedThrow(new InvalidOperationException("second")));

        var ex = await Assert.ThrowsAsync<AggregateException>(async () => await tracker.DisposeAsync());

        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task DisposeAsync_Parallel_DisposesAllTrackedItems()
    {
        var log = new List<string>();
        var tracker = AsyncDisposable.CreateTracker(DisposalOrder.Parallel);

        tracker.Track(new RecordingAsyncDisposable(log, "A"));
        tracker.Track(new RecordingAsyncDisposable(log, "B"));
        tracker.Track(new RecordingAsyncDisposable(log, "C"));

        await tracker.DisposeAsync();

        Assert.Equal(["A", "B", "C"], log.OrderBy(x => x));
    }

    [Fact]
    public async Task DisposeAsync_Parallel_CollectsAllExceptions()
    {
        // Regression test: Task.WhenAll only re-throws the first fault on await,
        // so DisposeParallel must explicitly gather every faulted task's exceptions
        // rather than relying on the one the await surfaces.
        var tracker = AsyncDisposable.CreateTracker(DisposalOrder.Parallel);

        tracker.Track(new RecordingAsyncDisposable([], "ok"));
        tracker.Track(DelayedThrow(new InvalidOperationException("first")));
        tracker.Track(DelayedThrow(new InvalidOperationException("second")));

        var ex = await Assert.ThrowsAsync<AggregateException>(async () => await tracker.DisposeAsync());

        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task Track_SyncDisposable_IsDisposedOnDisposeAsync()
    {
        var log = new List<string>();
        var tracker = AsyncDisposable.CreateTracker();

        tracker.Track(Disposable.Create(() => log.Add("sync")));

        await tracker.DisposeAsync();

        Assert.Equal(["sync"], log);
    }
}

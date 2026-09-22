using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class DisposableTrackerTests
{
    private sealed class RecordingDisposable(List<string> log, string name) : IDisposable
    {
        public void Dispose() => log.Add(name);
    }

    private sealed class ThrowingDisposable(Exception exception) : IDisposable
    {
        public void Dispose() => throw exception;
    }

    [Fact]
    public void Track_AfterDispose_Throws()
    {
        var tracker = Disposable.CreateTracker();
        tracker.Dispose();

        Assert.Throws<ObjectDisposedException>(() => tracker.Track(new RecordingDisposable([], "x")));
    }

    [Fact]
    public void Dispose_DisposesTrackedItemsInReverseOrder()
    {
        var log = new List<string>();
        var tracker = Disposable.CreateTracker();

        tracker.Track(new RecordingDisposable(log, "A"));
        tracker.Track(new RecordingDisposable(log, "B"));
        tracker.Track(new RecordingDisposable(log, "C"));

        tracker.Dispose();

        Assert.Equal(["C", "B", "A"], log);
    }

    [Fact]
    public void Dispose_CollectsAllExceptions_AndStillDisposesRemainingItems()
    {
        var log = new List<string>();
        var tracker = Disposable.CreateTracker();

        tracker.Track(new RecordingDisposable(log, "A"));
        tracker.Track(new ThrowingDisposable(new InvalidOperationException("first")));
        tracker.Track(new ThrowingDisposable(new InvalidOperationException("second")));
        tracker.Track(new RecordingDisposable(log, "D"));

        var ex = Assert.Throws<AggregateException>(tracker.Dispose);

        Assert.Equal(2, ex.InnerExceptions.Count);
        Assert.Equal(["D", "A"], log);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var log = new List<string>();
        var tracker = Disposable.CreateTracker();
        tracker.Track(new RecordingDisposable(log, "A"));

        tracker.Dispose();
        tracker.Dispose();

        Assert.Single(log);
    }
}

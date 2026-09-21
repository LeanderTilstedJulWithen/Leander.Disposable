using System.Diagnostics;
using Leander.Disposable;
namespace DisposableSample;

public static class AsyncTrackerSample
{
    public static async Task RunAsync(DisposalOrder order)
    {
        Console.WriteLine();
        Console.WriteLine($"============Async Tracker ({order})===============");
        await using var tracker = AsyncDisposable.CreateTracker(order);
        var sw = new Stopwatch();

        tracker.Track(AsynchronousCleanup("Resource1", TimeSpan.FromSeconds(0.3), sw));
        tracker.Track(SynchronousCleanup("Resource2", TimeSpan.FromSeconds(0.2), sw));
        tracker.Track(AsynchronousCleanup("Resource3", TimeSpan.FromSeconds(0.5), sw));
        tracker.Track(AsynchronousCleanup("Resource4", TimeSpan.FromSeconds(1), sw));
        tracker.Track(SynchronousCleanup("Resource5", TimeSpan.FromSeconds(0.8), sw));
        tracker.Track(AsynchronousCleanup("Resource6", TimeSpan.FromSeconds(0.6), sw));
        sw.Start();
    }

    public static IAsyncDisposable SynchronousCleanup(string id, TimeSpan delay, Stopwatch sw)
    {
        return AsyncDisposable.Create(() =>
        {
            Thread.Sleep(delay);
            Console.WriteLine($"Synchronous cleanup action executed for {id}. Elapsed milliseconds: {sw.ElapsedMilliseconds}");
            return ValueTask.CompletedTask;
        });
    }

    public static IAsyncDisposable AsynchronousCleanup(string id, TimeSpan delay, Stopwatch sw)
    {
        return AsyncDisposable.Create(async () =>
        {
            await Task.Delay(delay).ConfigureAwait(true);
            Console.WriteLine($"Asynchronous cleanup action executed for {id}. Elapsed milliseconds: {sw.ElapsedMilliseconds}");
        });
    }
}
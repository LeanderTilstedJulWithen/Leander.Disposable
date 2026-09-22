using System.Diagnostics;
using Leander.Disposable;

namespace DisposableSample;

// Demonstrates AsyncDisposable.CreateTracker's two disposal orders.
// Lifo disposes one at a time in reverse registration order; Parallel starts
// every disposal at once and waits for them all to finish.
internal static class AsyncTrackerSample
{
    public static async Task RunAsync(DisposalOrder order)
    {
        Console.WriteLine($"=== AsyncDisposable.CreateTracker ({order}) ===");

        await using var tracker = AsyncDisposable.CreateTracker(order);
        var sw = Stopwatch.StartNew();

        tracker.Track(AsynchronousCleanup("Resource1", TimeSpan.FromSeconds(0.2), sw));
        tracker.Track(AsynchronousCleanup("Resource2", TimeSpan.FromSeconds(0.8), sw));
        tracker.Track(SynchronousCleanup("Resource3", TimeSpan.FromSeconds(1), sw));
        tracker.Track(AsynchronousCleanup("Resource4", TimeSpan.FromSeconds(0.4), sw));
        tracker.Track(AsynchronousCleanup("Resource5", TimeSpan.FromSeconds(0.6), sw));

        // Disposal happens here, when the tracker goes out of scope.
    }

    private static IAsyncDisposable SynchronousCleanup(string id, TimeSpan delay, Stopwatch sw)
    {
        return AsyncDisposable.Create(() =>
        {
            Thread.Sleep(delay);
            Console.WriteLine($"  {id} closed at {sw.ElapsedMilliseconds}ms");
            return ValueTask.CompletedTask;
        });
    }

    private static IAsyncDisposable AsynchronousCleanup(string id, TimeSpan delay, Stopwatch sw)
    {
        return AsyncDisposable.Create(async () =>
        {
            await Task.Delay(delay);
            Console.WriteLine($"  {id} closed at {sw.ElapsedMilliseconds}ms");
        });
    }

    // Output (elapsed milliseconds vary between runs — this is illustrative, not exact):
    //
    // === AsyncDisposable.CreateTracker (Lifo) ===
    // Each resource is awaited to completion before the next starts, in reverse
    // registration order: Resource5, Resource4, Resource3, Resource2, Resource1
    // (roughly 600ms, 1000ms, 2000ms, 2800ms, 3000ms elapsed).
    //
    // === AsyncDisposable.CreateTracker (Parallel) ===
    // All resources are disposed concurrently, so they complete in an order driven
    // by their own delay rather than registration order. A synchronous cleanup
    // (Resource3) briefly blocks the loop, which means that Resource1 and Resource2 are delayed by 1 second.
    // The expected order is something like: Resource4, Resource5, Resource3, Resource1, Resource2 (400ms, 600ms, 1000ms, 1200ms, 1800ms elapsed).
}

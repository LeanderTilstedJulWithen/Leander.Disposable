using Leander.Disposable;
namespace DisposableSample;

public static class AsyncTrackerSample
{
    public static async Task RunAsync(DisposalOrder order)
    {
        Console.WriteLine($"============Async Tracker ({order})===============");
        await using var tracker = AsyncDisposable.CreateTracker(order);

        tracker.Track(AsynchronousCleanup("Resource1", TimeSpan.FromSeconds(2)));
        tracker.Track(SynchronousCleanup("Resource2"));
        tracker.Track(AsynchronousCleanup("Resource3", TimeSpan.FromSeconds(3)));
        tracker.Track(AsynchronousCleanup("Resource4", TimeSpan.FromSeconds(0.1)));
        tracker.Track(SynchronousCleanup("Resource5"));
        tracker.Track(AsynchronousCleanup("Resource6", TimeSpan.FromSeconds(1)));
    }

    public static IAsyncDisposable SynchronousCleanup(string id) => AsyncDisposable.Create(() =>
    {
        
        Console.WriteLine($"Synchronous cleanup action executed for {id}.");
        return ValueTask.CompletedTask;
    });

    public static IAsyncDisposable AsynchronousCleanup(string id, TimeSpan delay)
    {
        return AsyncDisposable.Create(async () =>
        {
            await Task.Delay(delay);
            Console.WriteLine($"Asynchronous cleanup action executed for {id}.");
        });
    }
}
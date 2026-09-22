using Leander.Disposable;

namespace DisposableSample;

// Demonstrates AsyncDisposable.Create wrapping a running background loop.
// Disposal signals cancellation and awaits the loop's exit, so by the time
// DisposeAsync returns, the loop has fully drained — no orphaned work.
internal static class AsyncCreateSample
{
    private sealed class Poller
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _loop;

        public Poller() => _loop = RunAsync(_cts.Token);

        public IAsyncDisposable AsDisposable() => AsyncDisposable.Create(async () =>
        {
            _cts.Cancel();
            await _loop; // wait for the loop to observe cancellation and exit
        });

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine("  Polling...");
                try
                {
                    await Task.Delay(150, token);
                }
                catch (OperationCanceledException)
                {
                    // expected on shutdown
                }
            }
            Console.WriteLine("  Loop exited.");
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("=== AsyncDisposable.Create — background loop shutdown ===");

        var poller = new Poller();
        var handle = poller.AsDisposable();

        await Task.Delay(350); // let a couple of iterations run

        Console.WriteLine("  Shutting down...");
        await handle.DisposeAsync(); // cancels the loop and awaits it before returning
        Console.WriteLine("  Shutdown complete.");
    }

    // Output (poll count may vary slightly with system timing):
    // === AsyncDisposable.Create — background loop shutdown ===
    //   Polling...
    //   Polling...
    //   Polling...
    //   Shutting down...
    //   Loop exited.
    //   Shutdown complete.
}

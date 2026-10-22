using Leander.Disposable;

namespace DisposableSample;

// Demonstrates Disposable.Empty and AsyncDisposable.Empty as opt-outs.
// Each API always returns a disposable so callers can write a plain `using`,
// but when the feature is switched off there is nothing to clean up — the
// shared no-op instance is returned instead of allocating a real one.
internal static class EmptySample
{
    private sealed class Tracer(bool enabled)
    {
        public IDisposable BeginSpan(string name)
        {
            if (!enabled) return Disposable.Empty;

            Console.WriteLine($"  [span start] {name}");
            return Disposable.Create(() => Console.WriteLine($"  [span end] {name}"));
        }
    }

    private sealed class Gate(bool enabled)
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async ValueTask<IAsyncDisposable> EnterAsync()
        {
            if (!enabled) return AsyncDisposable.Empty;

            await _semaphore.WaitAsync();
            Console.WriteLine("  [gate entered]");
            return AsyncDisposable.Create(() =>
            {
                _semaphore.Release();
                Console.WriteLine("  [gate released]");
                return ValueTask.CompletedTask;
            });
        }
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("=== Disposable.Empty / AsyncDisposable.Empty — opt-out ===");

        foreach (var enabled in new[] { true, false })
        {
            Console.WriteLine($"  Enabled: {enabled}");

            using (new Tracer(enabled).BeginSpan("load"))
            {
                Console.WriteLine("  Loading...");
            }

            await using (await new Gate(enabled).EnterAsync())
            {
                Console.WriteLine("  Working...");
            }
        }
    }

    // Output:
    // === Disposable.Empty / AsyncDisposable.Empty — opt-out ===
    //   Enabled: True
    //   [span start] load
    //   Loading...
    //   [span end] load
    //   [gate entered]
    //   Working...
    //   [gate released]
    //   Enabled: False
    //   Loading...
    //   Working...
}

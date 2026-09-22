using Leander.Disposable;
using Microsoft.Extensions.DependencyInjection;

namespace DisposableSample;

// Demonstrates AsyncDisposable.CreateTracker backing a generic factory built
// on ActivatorUtilities. If you reach for ActivatorUtilities to construct
// objects outside of DI-managed scopes, you're on the hook for disposing
// whatever comes back — this is one way to do that without writing a bespoke
// tracking list for every factory.
internal static class GenericFactorySample
{
    private sealed class GenericFactory(IServiceProvider serviceProvider) : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IAsyncDisposableTracker _tracker = AsyncDisposable.CreateTracker();

        public T CreateInstance<T>(params object[] args)
        {
            ObjectDisposedException.ThrowIf(_tracker.IsDisposed, typeof(GenericFactory));
            var result = ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
            _tracker.TrackIfDisposable(result!);
            return result;
        }

        public ValueTask DisposeAsync() => _tracker.DisposeAsync();
    }

    private sealed class ReportWriter(string name) : IDisposable
    {
        public void Dispose() => Console.WriteLine($"  '{name}' report writer closed.");
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("=== AsyncDisposable.CreateTracker — generic factory via ActivatorUtilities ===");

        var services = new ServiceCollection().BuildServiceProvider();
        await using var factory = new GenericFactory(services);

        factory.CreateInstance<ReportWriter>("Daily");
        factory.CreateInstance<ReportWriter>("Weekly");

        Console.WriteLine("  Leaving factory scope...");
    }

    // Output:
    // === AsyncDisposable.CreateTracker — generic factory via ActivatorUtilities ===
    //   Leaving factory scope...
    //   'Weekly' report writer closed.
    //   'Daily' report writer closed.
}

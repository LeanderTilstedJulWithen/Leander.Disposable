using Leander.Disposable;

namespace DisposableSample;

// Demonstrates AsyncDisposable.Wrap via an async connection guard.
// Mirrors WrapSample, but for a resource whose close requires an async
// handshake (e.g. flushing buffered writes) rather than a synchronous Dispose.
internal static class AsyncWrapSample
{
    private sealed class AsyncConnection : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("  Flushing buffered writes...");
            await Task.Delay(100);
            Console.WriteLine("  Connection closed.");
        }
    }

    private sealed class ConnectionGuard(IAsyncDisposable connection)
    {
        private readonly IAsyncDisposableState _handle = AsyncDisposable.Wrap(connection);

        public bool IsConnected => !_handle.IsDisposed;

        public ValueTask CloseAsync() => _handle.DisposeAsync();
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("=== AsyncDisposable.Wrap — async connection guard ===");

        var guard = new ConnectionGuard(new AsyncConnection());

        Console.WriteLine($"  Connected: {guard.IsConnected}");
        await guard.CloseAsync();
        Console.WriteLine($"  Connected: {guard.IsConnected}");
    }

    // Output:
    // === AsyncDisposable.Wrap — async connection guard ===
    //   Connected: True
    //   Flushing buffered writes...
    //   Connection closed.
    //   Connected: False
}

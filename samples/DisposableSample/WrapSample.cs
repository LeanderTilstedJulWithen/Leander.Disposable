using Leander.Disposable;

namespace DisposableSample;

// Demonstrates Disposable.Wrap via a connection guard.
// The guard does not own the connection's lifetime — it wraps it to expose
// IsDisposed so callers can check availability without controlling disposal.
internal static class WrapSample
{
    private sealed class ConnectionGuard(IDisposable connection)
    {
        private readonly IDisposableState _handle = Disposable.Wrap(connection);

        public bool IsConnected => !_handle.IsDisposed;

        public void Close() => _handle.Dispose();
    }

    public static void Run()
    {
        Console.WriteLine("=== Disposable.Wrap — connection guard ===");

        var connection = new MemoryStream();
        var guard = new ConnectionGuard(connection);

        Console.WriteLine($"  Connected: {guard.IsConnected}");
        guard.Close();
        Console.WriteLine($"  Connected: {guard.IsConnected}");
    }

    // Output:
    // === Disposable.Wrap — connection guard ===
    //   Connected: True
    //   Connected: False
}

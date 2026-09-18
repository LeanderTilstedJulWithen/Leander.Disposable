using Leander.Disposable;

// Demonstrates Disposable.Wrap via a connection guard.
// The guard does not own the connection's lifetime — it wraps it to expose
// IsDisposed so callers can check availability without controlling disposal.
internal static class WrapSample
{
    private sealed class ConnectionGuard
    {
        private readonly IDisposableState _handle;

        public ConnectionGuard(IDisposable connection) =>
            _handle = Disposable.Wrap(connection);

        public bool IsConnected => !_handle.IsDisposed;

        public void Close() => _handle.Dispose();
    }

    public static void Run()
    {
        Console.WriteLine("\n=== Disposable.Wrap — connection guard ===");

        var connection = new MemoryStream();
        var guard = new ConnectionGuard(connection);

        Console.WriteLine($"  Connected: {guard.IsConnected}");
        guard.Close();
        Console.WriteLine($"  Connected: {guard.IsConnected}");
    }
}

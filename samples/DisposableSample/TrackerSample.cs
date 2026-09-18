using Leander.Disposable;

// Demonstrates Disposable.CreateTracker via a scoped factory.
// The factory hands out connections to callers but retains ownership.
// Disposing the factory disposes every connection it has created.
internal static class TrackerSample
{
    private sealed class ConnectionFactory : IDisposable
    {
        private readonly IDisposableTracker _tracker = Disposable.CreateTracker();

        public MemoryStream OpenConnection(string name)
        {
            var connection = new MemoryStream();
            _tracker.Track(Disposable.Create(() => Console.WriteLine($"  Connection '{name}' closed.")));
            _tracker.Track(connection);
            return connection;
        }

        public void Dispose() => _tracker.Dispose();
    }

    public static void Run()
    {
        Console.WriteLine("\n=== Disposable.CreateTracker — scoped factory ===");

        using var factory = new ConnectionFactory();

        var a = factory.OpenConnection("A");
        var b = factory.OpenConnection("B");
        var c = factory.OpenConnection("C");

        a.WriteByte(1);
        b.WriteByte(2);
        c.WriteByte(3);

        Console.WriteLine("  Leaving factory scope...");
        // All connections disposed here in reverse order: C, B, A.
    }
}

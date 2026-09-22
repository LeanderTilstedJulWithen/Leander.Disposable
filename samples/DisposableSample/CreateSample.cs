using Leander.Disposable;

namespace DisposableSample;

// Demonstrates Disposable.Create via an event subscription pattern.
// Subscribe returns a disposal token — disposing it unsubscribes the handler.
internal static class CreateSample
{
    private sealed class MessageBus
    {
        private event Action<string>? _subscribers;

        public IDisposableState Subscribe(Action<string> handler)
        {
            _subscribers += handler;
            return Disposable.Create(() => _subscribers -= handler);
        }

        public void Publish(string message) => _subscribers?.Invoke(message);
    }

    public static void Run()
    {
        Console.WriteLine("=== Disposable.Create — subscription token ===");

        var bus = new MessageBus();

        using var subscription = bus.Subscribe(msg => Console.WriteLine($"  Received: {msg}"));

        bus.Publish("Hello");
        Console.WriteLine($"  Active: {!subscription.IsDisposed}");

        subscription.Dispose();

        bus.Publish("World");  // not received — handler was removed on dispose
        Console.WriteLine($"  Active: {!subscription.IsDisposed}");
    }

    // Output:
    // === Disposable.Create — subscription token ===
    //   Received: Hello
    //   Active: True
    //   Active: False
}

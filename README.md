# Leander.Disposable

A small .NET utility library for composing and managing disposable resources without writing boilerplate classes.

## Overview

Three factory methods cover the most common disposal patterns:

| Method | Returns | Use when |
|---|---|---|
| `Disposable.Create(action)` | `IDisposableState` | You have a cleanup callback and want a disposable handle |
| `Disposable.Wrap(disposable)` | `IDisposableState` | You have an existing `IDisposable` and need to query its disposal state |
| `Disposable.CreateTracker()` | `IDisposableTracker` | A scope or factory owns multiple resources and must clean them all up together |

`IDisposableState` extends `IDisposable` with a single `bool IsDisposed` property.  
`IDisposableTracker` extends `IDisposableState` with `Track(IDisposable)`.

### Thread safety

None of the types in this library are thread-safe. `Dispose`/`DisposeAsync` are safe to call more than once from a *single* thread (idempotent — later calls are no-ops), but calling `Dispose`, `DisposeAsync`, or `Track` concurrently from multiple threads on the same instance is not supported and may corrupt internal state or trigger a resource being disposed more than once. If you need a tracker shared across threads, synchronize access to it yourself.

---

## Disposable.Create

Wraps any cleanup action as a disposable. A natural fit for event subscriptions, where the returned token unsubscribes the handler when disposed.

```csharp
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
```

```csharp
var bus = new MessageBus();

using var subscription = bus.Subscribe(msg => Console.WriteLine($"Received: {msg}"));

bus.Publish("Hello");                    // → Received: Hello
Console.WriteLine(subscription.IsDisposed); // → False

subscription.Dispose();

bus.Publish("World");                    // (no output — handler was removed)
Console.WriteLine(subscription.IsDisposed); // → True
```

---

## Disposable.Wrap

Adds `IsDisposed` tracking to any existing `IDisposable`. Useful when a component holds a reference to a resource it did not create and needs to check availability without controlling disposal.

```csharp
private sealed class ConnectionGuard
{
    private readonly IDisposableState _handle;

    public ConnectionGuard(IDisposable connection) =>
        _handle = Disposable.Wrap(connection);

    public bool IsConnected => !_handle.IsDisposed;

    public void Close() => _handle.Dispose();
}
```

```csharp
var connection = new MemoryStream();
var guard = new ConnectionGuard(connection);

Console.WriteLine(guard.IsConnected); // → True
guard.Close();
Console.WriteLine(guard.IsConnected); // → False
```

---

## Disposable.CreateTracker

Collects resources and disposes them all in reverse acquisition order (LIFO) when the tracker is disposed. Well suited for factories with a bounded lifetime: the factory hands out resources to callers, and disposing the factory closes everything it opened.

```csharp
private sealed class ConnectionFactory : IDisposable
{
    private readonly IDisposableTracker _tracker = Disposable.CreateTracker();

    public MemoryStream OpenConnection(string name)
    {
        var connection = new MemoryStream();
        _tracker.Track(connection);
        return connection;
    }

    public void Dispose() => _tracker.Dispose();
}
```

```csharp
using var factory = new ConnectionFactory();

var a = factory.OpenConnection("A");
var b = factory.OpenConnection("B");
var c = factory.OpenConnection("C");

// On dispose: C closed, then B, then A.
```

If any tracked `Dispose()` call throws, disposal continues through the remaining resources. All exceptions are collected and rethrown together as an `AggregateException`.


## AsyncDisposable.CreateTracker

The tracker utility is also available in an async version. Here is an implementation of a very generic factory using ActivatorUtilities. If you for some reason decide to use ActivatorUtilities to create objects, then you are responsible for cleaning up your mess. This is one way to do this.

```csharp
public sealed class GenericFactory(IServiceProvider serviceProvider) : IAsyncDisposable
{
    private readonly IAsyncDisposableTracker _tracker = AsyncDisposable.CreateTracker();
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public T CreateInstance<T>(params object[] args)
    {
        ObjectDisposedException.ThrowIf(_tracker.IsDisposed, typeof(GenericFactory));
        var result = ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
        if (result is IAsyncDisposable asyncDisposable)
        {
            _tracker.Track(asyncDisposable);
        }
        else if (result is IDisposable disposable)
        {
            _tracker.Track(disposable);
        }

        return result;
    }

    public ValueTask DisposeAsync() => _tracker.DisposeAsync();
}
```

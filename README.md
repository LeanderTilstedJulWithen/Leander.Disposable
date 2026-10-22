# Leander.Disposable

A small .NET utility library for composing and managing disposable resources without writing boilerplate classes.

```
dotnet add package Leander.Disposable
```

## Why

Hand-rolling a disposable class means tracking a disposed flag, guarding against double-dispose, and — once you own more than one resource — cleaning them up in the right order and deciding what happens if one of them throws.

```csharp
public sealed class ReportScheduler : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly Timer _timer;
    private bool _disposed;

    public ReportScheduler(string path)
    {
        _watcher = new FileSystemWatcher(path);
        _timer = new Timer(_ => GenerateReport(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    private void GenerateReport() { /* ... */ }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var exceptions = new List<Exception>();
        try { _timer.Dispose(); } catch (Exception ex) { exceptions.Add(ex); }
        try { _watcher.Dispose(); } catch (Exception ex) { exceptions.Add(ex); }
        if (exceptions.Count > 0) throw new AggregateException(exceptions);
    }
}
```

`Disposable.CreateTracker()` covers the disposed flag, ordering, and exception aggregation, leaving just the resources themselves:

```csharp
public sealed class ReportScheduler : IDisposable
{
    private readonly IDisposableTracker _tracker = Disposable.CreateTracker();
    private readonly Timer _timer;
    private readonly FileSystemWatcher _watcher;

    public ReportScheduler(string path)
    {
        _watcher = new FileSystemWatcher(path);
        _tracker.Track(_watcher);
        _timer = new Timer(_ => GenerateReport(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        _tracker.Track(_timer);
    }

    private void GenerateReport() { /* ... */ }

    public void Dispose() => _tracker.Dispose();
}
```

## Overview

Eight factory members cover the common disposal patterns, in both synchronous and asynchronous flavors:

| Method | Returns | Use when |
|---|---|---|
| `Disposable.Create(action)` | `IDisposableState` | You have a cleanup callback and want a disposable handle |
| `Disposable.Wrap(disposable)` | `IDisposableState` | You have an existing `IDisposable` and need to query its disposal state |
| `Disposable.CreateTracker()` | `IDisposableTracker` | A scope or factory owns multiple resources and must clean them all up together |
| `Disposable.Empty` | `IDisposable` | An API requires a disposable but there is nothing to clean up (shared no-op singleton) |
| `AsyncDisposable.Create(action)` | `IAsyncDisposableState` | Same as `Disposable.Create`, but cleanup needs to `await` something |
| `AsyncDisposable.Wrap(disposable)` | `IAsyncDisposableState` | Same as `Disposable.Wrap`, for an existing `IAsyncDisposable` |
| `AsyncDisposable.CreateTracker(order?)` | `IAsyncDisposableTracker` | Same as `Disposable.CreateTracker`, with a choice of LIFO or parallel disposal order |
| `AsyncDisposable.Empty` | `IAsyncDisposable` | Same as `Disposable.Empty`, where an `IAsyncDisposable` is required |

`IDisposableState`/`IAsyncDisposableState` extend `IDisposable`/`IAsyncDisposable` with a single `bool IsDisposed` property.  
`IDisposableTracker`/`IAsyncDisposableTracker` extend those with `Track(...)`.  
The `Empty` instances are shared, so they deliberately expose no `IsDisposed`. A shared instance can't track disposal per owner, and `ThrowIfDisposed` guards built on it would never fire.

## Quick reference

### Restore state at the end of a scope

`Disposable.Create` turns any "undo" step into something `using` can drive:

```csharp
var previous = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Yellow;
using (Disposable.Create(() => Console.ForegroundColor = previous))
{
    Console.WriteLine("Warning: low disk space");
}
```

### Guard members after disposal

`ThrowIfDisposed` works on any `IDisposableState`/`IAsyncDisposableState`. Pass `this` so the exception names your type rather than the library's internal one:

```csharp
public void Enqueue(Job job)
{
    _tracker.ThrowIfDisposed(this); // ObjectDisposedException naming JobQueue, not the tracker
    ...
}
```

### Track things that might be disposable

`TrackIfDisposable` takes any `object` and tracks it only if it implements `IDisposable` (or, on an async tracker, `IAsyncDisposable`, which is preferred when both are implemented). It's useful when you create instances of types you don't control:

```csharp
var handler = Activator.CreateInstance(handlerType)!;
_tracker.TrackIfDisposable(handler);
```

An async tracker accepts both `IAsyncDisposable` and `IDisposable` through `Track`, so one tracker can own a mix of both.

### Choose the async disposal order

`AsyncDisposable.CreateTracker` disposes in reverse registration order (LIFO) by default. Pass `DisposalOrder.Parallel` when the resources don't depend on each other and you want to wait for all of them at once. You can also change the process-wide default:

```csharp
await using var tracker = AsyncDisposable.CreateTracker(DisposalOrder.Parallel);

// or, once at startup:
AsyncDisposable.DefaultDisposalOrder = DisposalOrder.Parallel;
```

### Opt out without allocating

```csharp
public IDisposable BeginScope(string name) =>
    _enabled ? Disposable.Create(() => EndScope(name)) : Disposable.Empty;
```

### Exceptions during disposal

A tracker always attempts to dispose *every* resource it holds, even if some of them throw. Once all have been attempted, any exceptions are rethrown together as a single `AggregateException`. With `DisposalOrder.Parallel`, failures from every concurrent disposal are collected the same way. The tracker counts as disposed after the first call either way, so a retry will not dispose anything twice.

### Idempotent disposal

`Dispose()`/`DisposeAsync()` on every type in this library can be called more than once safely — the first call runs your cleanup, every call after that is a no-op. You never need to guard your own `Dispose` method with a `_disposed` check before delegating to a tracker or a wrapped disposable: call it from as many code paths as you need (a `using` block *and* an explicit early-return path, say) without risking double cleanup. `IsDisposed` reflects whether the first call has already happened.

### Thread safety

None of the types in this library are thread-safe. Idempotency holds when `Dispose`/`DisposeAsync` is called more than once from a *single* thread, but calling `Dispose`, `DisposeAsync`, or `Track` concurrently from multiple threads on the same instance is not supported and may corrupt internal state or cause a resource to be disposed more than once. If you need a tracker shared across threads, synchronize access to it yourself.

## Samples

[`samples/DisposableSample`](samples/DisposableSample) is a runnable console project demonstrating all eight factory members — including LIFO vs. parallel async disposal ordering, and a generic factory built on `ActivatorUtilities`.

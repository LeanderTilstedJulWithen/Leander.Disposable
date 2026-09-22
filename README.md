# Leander.Disposable

A small .NET utility library for composing and managing disposable resources without writing boilerplate classes.

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
    private readonlt FileSystemWatcher _watcher;

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

Six factory methods cover the common disposal patterns, in both synchronous and asynchronous flavors:

| Method | Returns | Use when |
|---|---|---|
| `Disposable.Create(action)` | `IDisposableState` | You have a cleanup callback and want a disposable handle |
| `Disposable.Wrap(disposable)` | `IDisposableState` | You have an existing `IDisposable` and need to query its disposal state |
| `Disposable.CreateTracker()` | `IDisposableTracker` | A scope or factory owns multiple resources and must clean them all up together |
| `AsyncDisposable.Create(action)` | `IAsyncDisposableState` | Same as `Disposable.Create`, but cleanup needs to `await` something |
| `AsyncDisposable.Wrap(disposable)` | `IAsyncDisposableState` | Same as `Disposable.Wrap`, for an existing `IAsyncDisposable` |
| `AsyncDisposable.CreateTracker(order?)` | `IAsyncDisposableTracker` | Same as `Disposable.CreateTracker`, with a choice of LIFO or parallel disposal order |

`IDisposableState`/`IAsyncDisposableState` extend `IDisposable`/`IAsyncDisposable` with a single `bool IsDisposed` property.  
`IDisposableTracker`/`IAsyncDisposableTracker` extend those with `Track(...)`.

### Idempotent disposal

`Dispose()`/`DisposeAsync()` on every type in this library can be called more than once safely — the first call runs your cleanup, every call after that is a no-op. You never need to guard your own `Dispose` method with a `_disposed` check before delegating to a tracker or a wrapped disposable: call it from as many code paths as you need (a `using` block *and* an explicit early-return path, say) without risking double cleanup. `IsDisposed` reflects whether the first call has already happened.

### Thread safety

None of the types in this library are thread-safe. Idempotency holds when `Dispose`/`DisposeAsync` is called more than once from a *single* thread, but calling `Dispose`, `DisposeAsync`, or `Track` concurrently from multiple threads on the same instance is not supported and may corrupt internal state or cause a resource to be disposed more than once. If you need a tracker shared across threads, synchronize access to it yourself.

## Samples

[`samples/DisposableSample`](samples/DisposableSample) is a runnable console project demonstrating all six factory methods — including LIFO vs. parallel async disposal ordering, and a generic factory built on `ActivatorUtilities`.

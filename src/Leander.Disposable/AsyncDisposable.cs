namespace Leander.Disposable;

/// <summary>Factory for creating async disposable utility objects.</summary>
public static class AsyncDisposable
{
    /// <summary>Creates an async disposable that invokes <paramref name="action"/> exactly once on disposal.</summary>
    /// <param name="action">The async cleanup action to run on disposal.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static IAsyncDisposableState Create(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new AsyncDisposableAction(action);
    }

    /// <summary>Creates an async tracker that collects and disposes registered resources in reverse registration order.</summary>
    public static IAsyncDisposableTracker CreateTracker(DisposalOrder? order = null) => new AsyncDisposableTracker(order ?? DefaultDisposalOrder);

    /// <summary>Wraps an existing <see cref="IAsyncDisposable"/> to expose disposal state via <see cref="IAsyncDisposableState"/>.</summary>
    /// <param name="disposable">The disposable to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
    /// 
    public static IAsyncDisposableState Wrap(IAsyncDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        return new AsyncDisposableWrapper(disposable);
    }

    /// <summary>The <see cref="DisposalOrder"/> used by <see cref="CreateTracker"/> when no order is specified explicitly.</summary>
    public static DisposalOrder DefaultDisposalOrder { get; set; } = DisposalOrder.Lifo;
}
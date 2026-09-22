namespace Leander.Disposable;

/// <summary>
/// Controls the order in which <see cref="AsyncDisposable.CreateTracker"/> disposes tracked resources.
/// The synchronous <see cref="Disposable.CreateTracker"/> tracker only supports LIFO ordering.
/// </summary>
public enum DisposalOrder
{
    /// <summary>Disposes resources in reverse registration order (LIFO).</summary>
    Lifo = 1,

    /// <summary>Disposes resources in parallel. </summary>
    Parallel = 2
}
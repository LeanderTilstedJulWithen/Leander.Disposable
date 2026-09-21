namespace Leander.Disposable;

public enum DisposalOrder
{
    /// <summary>Disposes resources in reverse registration order (LIFO).</summary>
    Lifo = 1,

    /// <summary>Disposes resources in parallel. </summary>
    Parallel = 2
}
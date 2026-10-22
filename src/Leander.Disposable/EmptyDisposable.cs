namespace Leander.Disposable;

internal sealed class EmptyDisposable : IDisposable
{
    internal static readonly EmptyDisposable Instance = new();

    private EmptyDisposable() { }

    public void Dispose() { }
}

namespace Leander.Disposable;

internal sealed class EmptyAsyncDisposable : IAsyncDisposable
{
    internal static readonly EmptyAsyncDisposable Instance = new();

    private EmptyAsyncDisposable() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

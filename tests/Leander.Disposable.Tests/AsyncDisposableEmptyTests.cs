using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class AsyncDisposableEmptyTests
{
    [Fact]
    public void Empty_ReturnsSameInstance()
    {
        Assert.Same(AsyncDisposable.Empty, AsyncDisposable.Empty);
    }

    [Fact]
    public void Empty_DoesNotExposeDisposalState()
    {
        Assert.IsNotAssignableFrom<IAsyncDisposableState>(AsyncDisposable.Empty);
    }

    [Fact]
    public async Task DisposeAsync_CanBeCalledRepeatedly()
    {
        var disposable = AsyncDisposable.Empty;

        await disposable.DisposeAsync();
        await disposable.DisposeAsync();
    }
}

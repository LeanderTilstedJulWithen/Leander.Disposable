using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class AsyncDisposableCreateTests
{
    [Fact]
    public void Create_ThrowsOnNullAction()
    {
        Assert.Throws<ArgumentNullException>(() => AsyncDisposable.Create(null!));
    }

    [Fact]
    public async Task DisposeAsync_InvokesActionExactlyOnce()
    {
        var callCount = 0;
        var disposable = AsyncDisposable.Create(() =>
        {
            callCount++;
            return ValueTask.CompletedTask;
        });

        await disposable.DisposeAsync();
        await disposable.DisposeAsync();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task IsDisposed_ReflectsState()
    {
        var disposable = AsyncDisposable.Create(() => ValueTask.CompletedTask);

        Assert.False(disposable.IsDisposed);
        await disposable.DisposeAsync();
        Assert.True(disposable.IsDisposed);
    }
}

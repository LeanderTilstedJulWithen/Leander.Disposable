using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class AsyncDisposableWrapTests
{
    private sealed class TrackingAsyncDisposable : IAsyncDisposable
    {
        public int DisposeCount { get; private set; }

        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void Wrap_ThrowsOnNullDisposable()
    {
        Assert.Throws<ArgumentNullException>(() => AsyncDisposable.Wrap(null!));
    }

    [Fact]
    public async Task DisposeAsync_DelegatesToWrappedInstanceExactlyOnce()
    {
        var inner = new TrackingAsyncDisposable();
        var wrapper = AsyncDisposable.Wrap(inner);

        await wrapper.DisposeAsync();
        await wrapper.DisposeAsync();

        Assert.Equal(1, inner.DisposeCount);
    }

    [Fact]
    public async Task IsDisposed_ReflectsState()
    {
        var wrapper = AsyncDisposable.Wrap(new TrackingAsyncDisposable());

        Assert.False(wrapper.IsDisposed);
        await wrapper.DisposeAsync();
        Assert.True(wrapper.IsDisposed);
    }
}

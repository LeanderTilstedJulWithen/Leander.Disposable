using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class DisposableWrapTests
{
    private sealed class TrackingDisposable : IDisposable
    {
        public int DisposeCount { get; private set; }
        public void Dispose() => DisposeCount++;
    }

    [Fact]
    public void Wrap_ThrowsOnNullDisposable()
    {
        Assert.Throws<ArgumentNullException>(() => Disposable.Wrap(null!));
    }

    [Fact]
    public void Dispose_DelegatesToWrappedInstanceExactlyOnce()
    {
        var inner = new TrackingDisposable();
        var wrapper = Disposable.Wrap(inner);

        wrapper.Dispose();
        wrapper.Dispose();

        Assert.Equal(1, inner.DisposeCount);
    }

    [Fact]
    public void IsDisposed_ReflectsState()
    {
        var wrapper = Disposable.Wrap(new TrackingDisposable());

        Assert.False(wrapper.IsDisposed);
        wrapper.Dispose();
        Assert.True(wrapper.IsDisposed);
    }
}

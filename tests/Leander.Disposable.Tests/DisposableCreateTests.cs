using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class DisposableCreateTests
{
    [Fact]
    public void Create_ThrowsOnNullAction()
    {
        Assert.Throws<ArgumentNullException>(() => Disposable.Create(null!));
    }

    [Fact]
    public void Dispose_InvokesActionExactlyOnce()
    {
        var callCount = 0;
        var disposable = Disposable.Create(() => callCount++);

        disposable.Dispose();
        disposable.Dispose();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void IsDisposed_ReflectsState()
    {
        var disposable = Disposable.Create(() => { });

        Assert.False(disposable.IsDisposed);
        disposable.Dispose();
        Assert.True(disposable.IsDisposed);
    }
}

using Leander.Disposable;

namespace Leander.Disposable.Tests;

public class DisposableEmptyTests
{
    [Fact]
    public void Empty_ReturnsSameInstance()
    {
        Assert.Same(Disposable.Empty, Disposable.Empty);
    }

    [Fact]
    public void Empty_DoesNotExposeDisposalState()
    {
        Assert.IsNotAssignableFrom<IDisposableState>(Disposable.Empty);
    }

    [Fact]
    public void Dispose_CanBeCalledRepeatedly()
    {
        var disposable = Disposable.Empty;

        disposable.Dispose();
        disposable.Dispose();
    }
}

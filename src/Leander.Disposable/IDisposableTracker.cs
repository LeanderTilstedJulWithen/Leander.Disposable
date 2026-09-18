namespace Leander.Disposable;

public interface IDisposableTracker : IDisposableState
{
    public void Track(IDisposable disposable);
}
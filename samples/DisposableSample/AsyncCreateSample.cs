using Leander.Disposable;

namespace DisposableSample;

public static class AsyncCreateSample
{
    public static IAsyncDisposable Adapt(IDisposable disposable)
    {
        return AsyncDisposable.Create(() =>
        {
            disposable.Dispose();
            return ValueTask.CompletedTask;
        });
    }
    public static async Task RunAsync()
    {
        Console.WriteLine("============Async Create===============");
        var memoryStream = new MemoryStream();
        
        var asyncDisposable = AsyncDisposable.Create(async () =>
        {
            await Task.Delay(1000);
            Console.WriteLine("Async cleanup action executed.");
        });

        await asyncDisposable.DisposeAsync();
    }
}
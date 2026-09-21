using DisposableSample;
using Leander.Disposable;

CreateSample.Run();
WrapSample.Run();
TrackerSample.Run();

Console.WriteLine("Async samples start here");
await AsyncCreateSample.RunAsync();
await AsyncTrackerSample.RunAsync(DisposalOrder.Lifo);
await AsyncTrackerSample.RunAsync(DisposalOrder.Parallel);

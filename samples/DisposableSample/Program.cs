using DisposableSample;
using Leander.Disposable;

CreateSample.Run();
Console.WriteLine();
WrapSample.Run();
Console.WriteLine();
TrackerSample.Run();
Console.WriteLine();

await AsyncCreateSample.RunAsync();
Console.WriteLine();
await AsyncWrapSample.RunAsync();
Console.WriteLine();
await AsyncTrackerSample.RunAsync(DisposalOrder.Lifo);
Console.WriteLine();
await AsyncTrackerSample.RunAsync(DisposalOrder.Parallel);
Console.WriteLine();
await GenericFactorySample.RunAsync();

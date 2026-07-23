using System.IO;
using System.Collections.Frozen;
using Layer2.Web;
 
var tcs = new TaskCompletionSource();

// Gracefully handle exit signals (Ctrl+C or process termination)
AppDomain.CurrentDomain.ProcessExit += (s, e) => tcs.TrySetResult();
Console.CancelKeyPress += (sender, receiver) =>
{
    receiver.Cancel = true; // Prevent abrupt termination
    tcs.TrySetResult();
};

Console.WriteLine("Engine running... Press Ctrl+C to stop.");

// Keeps the app alive forever asynchronously without consuming CPU or blocking threads!
await tcs.Task;

Console.WriteLine("Shutting down cleanly.");
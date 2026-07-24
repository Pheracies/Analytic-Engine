using System.IO;
using System.Collections.Frozen;
using Web = Layer2.Web;
using Items = Layer1.Items;
using Layer2.Web;
using System.Numerics;
using Layer1.Items;

var tcs = new TaskCompletionSource();

// Gracefully handle exit signals (Ctrl+C or process termination)
AppDomain.CurrentDomain.ProcessExit += (s, e) => tcs.TrySetResult();
Console.CancelKeyPress += (sender, receiver) =>
{
    
    receiver.Cancel = true; // Prevent abrupt termination
    tcs.TrySetResult();
};

Console.WriteLine("Engine running... Press Ctrl+C to stop.");
List<ItemCategories> newList = new();

newList.Add(ItemCategories.Weapon);
newList.Add(ItemCategories.Wealth);

Webpage newWebpage = new();

Request newReq = new Request("Gold Sword",1, newList,ActionType.Add);
newWebpage.Send(newReq);

// Keeps the app alive forever asynchronously without consuming CPU or blocking threads!
await tcs.Task;

Console.WriteLine("Shutting down cleanly at " + DateTime.Now.ToLocalTime());
/*
Pheracies, 7/23/26
I wrote code, had AI document it 
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Layer1.Items;

namespace Layer2.Web;


public enum Result
{
    Success,
    Fail
}

// 1. DTO / Payload Class (Pure Data Model)
public class Request
{
    
    public string Type { get; set; } = string.Empty;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public int Amount { get; set; }
    public List<ItemCategories> Categories { get; set; } = new();
    
    public long LastUpdated { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    // Parameterless constructor required for JSON deserialization
    public Request() { }

    public Request(string itemType, int itemAmount, List<ItemCategories> itemCategories)
    {
        Type = ItemValidator.CleanString(itemType);
        Amount = itemAmount;
        Categories = itemCategories ?? new();
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    public static void overrideRequest(ref Request req1, ref Request req2, Func<Request, Request, Request> func)
    {
        if (func == null)
        {
            throw new Exception("func is null");    
        }
        Request mergedResult = func(req1, req2);
        req1.Amount = mergedResult.Amount;
        req1.Timestamp = mergedResult.Timestamp;
        req1.Categories = mergedResult.Categories;
        req1.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

// 2. The RemoteEvent Hub (Handles incoming/outgoing SignalR events)
public class RequestHub : Hub
{
    // Pheracies, 7/23/26
    // Shared static session list to store requests in memory for the current execution
    private static readonly Dictionary<string,Request> requests = new();

    // Client calls this like a RemoteEvent: FireServer("SendRequest", request)
    public async Task SendRequest(Request req)
    {
        Console.WriteLine($"[SignalR] Received request for {req.Type} (Amount: {req.Amount})");
        
        req.Type = ItemValidator.CleanString(req.Type);
        req.Amount = Math.Clamp(req.Amount, 1, 3000);
        
        // Save to our static list
        Request broadcastItem = req;
        if (requests.ContainsKey(req.Type))
        {
            var existingRequest = requests[req.Type];
            Request.overrideRequest(ref existingRequest, ref req, (existing, newReq) =>
            {
                existing.Amount += newReq.Amount;
                existing.Categories = newReq.Categories;
                return existing;
            });
            requests[req.Type] = existingRequest;
            broadcastItem = existingRequest;
        }
        else
        {
            requests[req.Type] = req;
        }

        // Broadcast to all clients
        await Clients.All.SendAsync("OnRequestReceived", broadcastItem);
    }

    public override async Task OnConnectedAsync()
    {
        // Pheracies, 7/23/26
        // Handle client connection lifecycle, log PID, and sync connection history
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"[SignalR] 🟢 Client Connected! ConnectionId: {connectionId}");
        
        // Send a welcome message only to the client that just connected
        await Clients.Caller.SendAsync("OnSystemMessage", $"Welcome! Your session ID is {connectionId}");
        
        // Loop through the history and send it to the caller
        foreach (var request in requests.Values)
        {
            await Clients.Caller.SendAsync("OnRequestReceived", request);
        }
        
        await base.OnConnectedAsync();
    }

    // Fires when a client disconnects!
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"[SignalR] 🔴 Client Disconnected: {connectionId}. Reason: {exception?.Message ?? "None"}");
        await base.OnDisconnectedAsync(exception);
    }
}

// 3. Webpage Engine & Server Host
public class Webpage
{
    public void ParseRequest(ref Request req)
    {
        req.Amount = Math.Clamp(req.Amount, 1, 3_000);
    }

    public Result Send(Request req)
    {
        ParseRequest(ref req);
        DateTimeOffset reqDate = DateTimeOffset.FromUnixTimeMilliseconds(req.Timestamp);

        Console.WriteLine($"Sending {req.Type} (Amount: {req.Amount}) over");
        Console.WriteLine($"Request Sent: {reqDate.LocalDateTime}");
        Console.WriteLine("ItemCategories:");
        
        foreach (var category in req.Categories)
        {
            Console.WriteLine($" - {category}");
        }

        return Result.Success;
    }

    public Webpage()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "AnalyticEngine.Api"
        });

        builder.Services.AddCors(options =>
        {
            
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173",
                "https://analytic-engine.vercel.app/")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Required for SignalR
            });
        });
        // MUST register SignalR services!
        builder.Services.AddSignalR();

        var app = builder.Build();
        app.UseCors();
        // Map the Hub (RemoteEvent Receiver Endpoint)
        app.MapHub<RequestHub>("/send");

        // GET Root
        app.MapGet("/", () => "Hello World! Your C# web server is live.");

        // GET Item Endpoint
        app.MapGet("/api/items/{id:int}", (int id) =>
        {
            var item = new 
            { 
                Id = id, 
                Name = $"Item {id}", 
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
            };
            
            return Results.Ok(item);
        });

        // POST Endpoint using the Request payload
        app.MapPost("/api/items", (Request request) =>
        {
            Send(request); // Run your internal parse & send logic
            return Results.Created($"/api/items/1", new { Status = "Success", ReceivedType = request.Type });
        });

        // Start server asynchronously in background thread
        Task.Run(() => app.Run());
    }
}
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

    // Parameterless constructor required for JSON deserialization
    public Request() { }

    public Request(string itemType, int itemAmount, List<ItemCategories> itemCategories)
    {
        Type = ItemValidator.CleanString(itemType);
        Amount = itemAmount;
        Categories = itemCategories ?? new();
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

// 2. The RemoteEvent Hub (Handles incoming/outgoing SignalR events)
public class RequestHub : Hub
{
    // Client calls this like a RemoteEvent: FireServer("SendRequest", request)
    public async Task SendRequest(Request req)
    {
        Console.WriteLine($"[SignalR] Received request for {req.Type} (Amount: {req.Amount})");

        // Broadcast to all clients (RemoteEvent:FireAllClients)
        await Clients.All.SendAsync("OnRequestReceived", req);
    }
     public override async Task OnConnectedAsync()
    {
        // You can get the client's unique Connection ID using 'Context'
        string connectionId = Context.ConnectionId;
        
        Console.WriteLine($"[SignalR] 🟢 Client Connected! ConnectionId: {connectionId}");
        
        // Example action: Send a direct greeting only to the client that just connected
        await Clients.Caller.SendAsync("OnSystemMessage", $"Welcome! Your session ID is {connectionId}");
        
        // MUST call the base method at the end so SignalR registers the connection!
        await base.OnConnectedAsync();
    }
    // 2. Fires when a client disconnects!
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        
        Console.WriteLine($"[SignalR] 🔴 Client Disconnected: {connectionId}. Reason: {exception?.Message ?? "None"}");
        
        // MUST call the base method at the end
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
                policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
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
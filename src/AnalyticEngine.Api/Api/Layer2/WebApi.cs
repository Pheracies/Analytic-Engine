
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Layer1.Items;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;


namespace Layer2.Web;
public enum Result
{
    Success,
    Fail
}
public class Webpage
{
    
    public void ParseRequest(ref Request req)
    {
        req.Amount = Math.Clamp(req.Amount,1,3 * (int)Math.Pow(10,3));
        
    }

    public Result Send(Request req)
    {
        ParseRequest(ref req);
        DateTimeOffset reqDate = DateTimeOffset.FromUnixTimeMilliseconds(req.Timestamp);
        //Do this sometime
        Console.WriteLine("Sending " + req.Type + "(Amount: " + req.Amount.ToString() + ")" + " over");
        Console.WriteLine("Request Sent: " + reqDate.LocalDateTime);
        Console.Write("ItemCategories: ");
        foreach (var category in req.Categories)
        {
            Console.WriteLine(category.ToString());
        }
        return Result.Success;
    }
    public Webpage()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "Analytical Engine"
        });

// Add services to the container here (e.g., builder.Services.AddControllers())

var app = builder.Build();

// 2. Define HTTP Endpoints (Routes)

// GET request to root "/"
app.MapGet("/", () => "Hello World! Your C# web server is live.");

// GET request with a route parameter and returning JSON automatically
app.MapGet("/api/items/{id:int}", (int id) =>
{
    var item = new 
    { 
        Id = id, 
        Name = $"Item {id}", 
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
    };
    
    return Results.Ok(item); // Returns HTTP 200 with JSON payload
});

// POST request receiving a JSON body
app.MapPost("/api/items", (Request request) =>
{
    return Results.Created($"/api/items/1", new { Status = "Success", ReceivedType = request.Type });
});

// 3. Start the Web Server (By default listens on http://localhost:5000 or 5200)
Task.Run(() =>
{
    app.Run();
});
    }
}
public class Request{
    public string Type;
    public long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public int Amount;
    public List<ItemCategories> Categories;
    public Request(string ItemType,int ItemAmount,List<ItemCategories> Item_Categories)
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Type = NameValidator.CleanString(ItemType);
        Amount = ItemAmount;
        Categories = Item_Categories;
        Timestamp = unixTimestamp;
    }
};
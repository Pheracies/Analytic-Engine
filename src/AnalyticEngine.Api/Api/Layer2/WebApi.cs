
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Layer1.Items;


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
        ref var Amnt = ref req.Amount;
        if (Amnt is int)
        {
            
        } 
        else
        {
            throw new InvalidCastException("Amount can't be cast to int");
        }
        Amnt = Math.Clamp(Amnt,1,3 * (int)Math.Pow(10,3));
        

        req.Amount = Amnt;
        
    }

    public Result Send(Request req)
    {
        ParseRequest(ref req);
        //Do this sometime
        Console.WriteLine("Sending " + req.Type + " over");
        Console.Write("ItemCategories: ");
        foreach (var category in req.Categories)
        {
            Console.WriteLine(category.ToString());
        }
        return Result.Success;
    }
}
public class Request{
    public string Type;
    public long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public int Amount;
    public List<ItemCategories> Categories;
    public Request(string ItemType,int ItemAmount,List<ItemCategories> Item_Categories)
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Type = NameValidator.CleanString(ItemType);
        Amount = ItemAmount;
        Categories = Item_Categories;
        Timestamp = unixTimestamp;
    }
};
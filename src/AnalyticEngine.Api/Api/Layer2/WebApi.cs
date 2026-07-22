
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Layer1.Items;


namespace Layer2.Web;
public record Request{
    public string Type;
    public long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public double Amount;
    Vector<ItemCategories> Categories;
    Request(string ItemType,double ItemAmount,Vector<ItemCategories> Item_Categories)
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Type = NameValidator.CleanString(ItemType);
        Amount = ItemAmount;
        Categories = Item_Categories;
        Timestamp = unixTimestamp;
    }
};
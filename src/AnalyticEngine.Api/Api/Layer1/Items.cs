using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;


namespace Layer1.Items;
public enum ItemCategories
{
    Weapon,
    Accessory,
    Wealth,
}

// 1. Static list/set for disallowed names
public static class NameValidator
{
    private static readonly HashSet<string> DisallowedNames = new()
    {
        "GOD",
        "ADMIN",
        "ROOT"
    };

    // Clean string parser method
    public static string CleanString(string item)
    {
        // Check if disallowed or explicitly 'GOD'
        if (DisallowedNames.Contains(item.ToUpper()))
        {
            return "UNKNOWN"; // Fallback value
        }

        return item;
    }
}

// 2. Primary Constructor Record syntax (Modern C# shorthand)

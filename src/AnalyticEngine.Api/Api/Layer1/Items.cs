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
    Utility
}

// 1. Static list/set for disallowed names
public static class ItemValidator
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
        // 1. Safe null/empty check first!
        if (string.IsNullOrWhiteSpace(item))
        {
            return "UNKNOWN";
        }

        // 2. Perform operations safely after verifying item is not null
        string upperItem = item.Trim().ToUpper();
        bool valid = true;

        // 3. Admin names check
        if (DisallowedNames.Contains(upperItem))
        {
            valid = false;
        }

        // 4. Basic XSS / HTML tags check
        if (upperItem.Contains("<SCRIPT") || upperItem.Contains("JAVASCRIPT:"))
        {
            valid = false;
        }

        // 5. Apply fallback if invalid, otherwise trim and check length
        if (!valid)
        {
            return "UNKNOWN";
        }

        // 6. Enforce length limit of 100 characters
        if (item.Length > 100)
        {
            item = item.Substring(0, 100);
        }

        return item.Trim();
    }
}

// 2. Primary Constructor Record syntax (Modern C# shorthand)

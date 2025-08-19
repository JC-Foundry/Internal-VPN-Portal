using System.Text.RegularExpressions;

namespace VPN_Portal.Extensions;

public static class EnumExtensions
{
    public static List<(string Name, int Value)> GetAllOptions<T>(this T _) 
        where T : struct, Enum
        => Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => (e.ToString(), Convert.ToInt32(e)))
            .ToList();
    
    public static string ToSpacedString(this Enum value)
    {
        var name = value.ToString();
        return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }
}
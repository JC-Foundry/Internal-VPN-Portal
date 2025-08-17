using System.Reflection;

namespace VPN_Portal.Extensions;

public class ConstExtensions
{
    public static Dictionary<string, object> GetAllConsts<T>()
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly) // const = literal, not initonly (readonly)
            .ToDictionary(f => f.Name, f => f.GetRawConstantValue()!);
    }
}
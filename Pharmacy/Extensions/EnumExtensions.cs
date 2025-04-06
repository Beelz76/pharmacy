using System.ComponentModel;
using System.Reflection;

namespace Pharmacy.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var attribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? enumValue.ToString();
    }
    
    public static IEnumerable<(TEnum, string)> GetEnumDetails<TEnum>() where TEnum : Enum
    {
        return typeof(TEnum)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(x =>
            {
                var t = (TEnum)x.GetValue(null)!;
                return (t, x.GetCustomAttribute<DescriptionAttribute>()!.Description);
            });
    }

}
namespace Pharmacy.Helpers;

public static class TypeValidationHelper
{
    public static bool IsValidType(string value, string expectedType)
    {
        return expectedType.ToLower() switch
        {
            "string" => !string.IsNullOrWhiteSpace(value),
            "number" => decimal.TryParse(value, out _),
            "integer" => int.TryParse(value, out _),
            "boolean" => bool.TryParse(value, out _),
            "date" => DateTime.TryParse(value, out _),
            _ => true
        };
    }
}
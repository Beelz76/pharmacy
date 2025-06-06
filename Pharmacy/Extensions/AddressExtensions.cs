using Pharmacy.Database.Entities;

namespace Pharmacy.Extensions;

public static class AddressExtensions
{
    public static string? FormatAddress(UserAddress? userAddress)
    {
        if (userAddress is null)
        {
            return null;
        }
        
        var a = userAddress.Address;
        var parts = new[]
        {
            a.Region,
            a.City,
            a.Suburb,
            a.Street,
            a.HouseNumber,
            string.IsNullOrWhiteSpace(userAddress.Entrance) ? null : $"подъезд {userAddress.Entrance}",
            string.IsNullOrWhiteSpace(userAddress.Floor) ? null : $"этаж {userAddress.Floor}",
            string.IsNullOrWhiteSpace(userAddress.Apartment) ? null : $"кв. {userAddress.Apartment}",
            a.Postcode
        };
        return string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
    
    public static string? FormatAddress(Address? address)
    {
        if (address is null)
        {
            return null;
        }
        
        var parts = new[]
        {
            address.Region,
            address.City,
            address.Suburb,
            address.Street,
            address.HouseNumber,
            address.Postcode
        };
        return string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
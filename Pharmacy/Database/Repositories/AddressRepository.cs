using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly PharmacyDbContext _context;

    public AddressRepository(PharmacyDbContext context)
    {
        _context = context;
    }
    
    public async Task<Address> GetOrCreateAddressAsync(Address address)
    {
        var existing = await _context.Addresses.FirstOrDefaultAsync(x =>
            (x.OsmId != null && x.OsmId == address.OsmId) || (
                x.Street == address.Street &&
                x.HouseNumber == address.HouseNumber &&
                Math.Abs(x.Latitude - address.Latitude) < 0.0001 &&
                Math.Abs(x.Longitude - address.Longitude) < 0.0001
            )
        );

        if (existing != null)
            return existing;

        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return address;
    }
}
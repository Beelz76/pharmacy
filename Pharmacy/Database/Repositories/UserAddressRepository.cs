using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly PharmacyDbContext _context;

    public UserAddressRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAddress>> GetByUserIdAsync(int userId)
    {
        return await _context.UserAddresses
            .Include(x => x.Address)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserAddress?> GetByIdAsync(int userId, int addressId)
    {
        return await _context.UserAddresses
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == addressId);
    }

    public async Task AddAsync(UserAddress address)
    {
        await _context.UserAddresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAddress address)
    {
        _context.UserAddresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserAddress address)
    {
        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using Pharmacy.Data.Repositories.Interfaces;
using Pharmacy.Models.Entities;

namespace Pharmacy.Data.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    private readonly PharmacyDbContext _context;
    public UserRepository(PharmacyDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        return await _context.Users.AnyAsync(m => m.Email == email && (!excludeId.HasValue || m.Id != excludeId.Value));
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
    }
    
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
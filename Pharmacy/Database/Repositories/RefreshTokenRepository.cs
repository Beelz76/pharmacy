using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly PharmacyDbContext _context;

    public RefreshTokenRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RemoveExpiredAsync(DateTime now)
    {
        var expired = await _context.RefreshTokens.Where(t => t.ExpiresAt <= now).ToListAsync();
        if (expired.Count == 0) return;
        _context.RefreshTokens.RemoveRange(expired);
        await _context.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Repositories;

public class EmailVerificationCodeRepository : BaseRepository, IEmailVerificationCodeRepository
{
    private readonly PharmacyDbContext _context;

    public EmailVerificationCodeRepository(PharmacyDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<EmailVerificationCode?> GetAsync(string email, string code, VerificationPurposeEnum purpose, DateTime now)
    {
        return await _context.EmailVerificationCodes
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.Code == code &&
                x.Purpose == purpose &&
                !x.IsUsed &&
                x.ExpiresAt > now);
    }

    public async Task<List<EmailVerificationCode>> GetAllActiveAsync(int userId, VerificationPurposeEnum purpose, DateTime now)
    {
        return await _context.EmailVerificationCodes
            .Where(x => x.UserId == userId && 
                        x.Purpose == purpose && 
                        !x.IsUsed && 
                        x.ExpiresAt > now)
            .ToListAsync();
    }

    
    public async Task<EmailVerificationCode?> GetLatestUsedAsync(int userId, VerificationPurposeEnum purpose, DateTime now)
    {
        return await _context.EmailVerificationCodes
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Purpose == purpose &&
                x.IsUsed &&
                x.ExpiresAt > now);
    }

    public async Task<bool> ExistsAsync(string email, string code, VerificationPurposeEnum purpose, DateTime now)
    {
        return await _context.EmailVerificationCodes
            .AnyAsync(x =>
                x.Email == email &&
                x.Code == code &&
                x.Purpose == purpose &&
                !x.IsUsed &&
                x.ExpiresAt > now);
    }
    
    public async Task AddAsync(EmailVerificationCode code)
    {
        await _context.EmailVerificationCodes.AddAsync(code);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(EmailVerificationCode code)
    {
        _context.EmailVerificationCodes.Update(code);
        await _context.SaveChangesAsync();
    }
}
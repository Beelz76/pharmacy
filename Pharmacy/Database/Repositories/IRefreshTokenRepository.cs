using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RemoveExpiredAsync(DateTime now);
}
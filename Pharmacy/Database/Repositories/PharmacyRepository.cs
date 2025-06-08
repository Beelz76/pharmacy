using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class PharmacyRepository : IPharmacyRepository
{
    private readonly PharmacyDbContext _context;

    public PharmacyRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Entities.Pharmacy pharmacy)
    {
        await _context.Pharmacies.AddAsync(pharmacy);
        await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<Entities.Pharmacy>> GetAllAsync()
    {
        return await _context.Pharmacies
            .Include(p => p.Address)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Entities.Pharmacy?> GetByIdAsync(int id)
    {
        return await _context.Pharmacies
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Entities.Pharmacy?> GetByOsmAndCoordinatesAsync(string name, string? osmId, double latitude, double longitude)
    {
        return await _context.Pharmacies
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p =>
                p.Name == name &&
                p.Address.OsmId == osmId &&
                Math.Abs(p.Address.Latitude - latitude) < 0.0001 &&
                Math.Abs(p.Address.Longitude - longitude) < 0.0001);
    }
    
    public async Task<bool> ExistsAsync(string name, string? osmId, double latitude, double longitude)
    {
        return await _context.Pharmacies
            .Include(p => p.Address)
            .AnyAsync(p =>
                p.Name == name &&
                (
                    (osmId != null && p.Address.OsmId == osmId) ||
                    (osmId == null && 
                     Math.Abs(p.Address.Latitude - latitude) < 0.0001 && 
                     Math.Abs(p.Address.Longitude - longitude) < 0.0001)
                ));
    }
    
    public async Task<bool> ExistsByIdAsync(int pharmacyId)
    {
        return await _context.Pharmacies.AnyAsync(p => p.Id == pharmacyId);
    }
    
    public async Task<Entities.Pharmacy?> GetNearestAsync(double latitude, double longitude)
    {
        return await _context.Pharmacies
            .Include(p => p.Address)
            .Where(p => p.IsActive)
            .OrderBy(p =>
                Math.Pow(p.Address.Latitude - latitude, 2) + Math.Pow(p.Address.Longitude - longitude, 2))
            .FirstOrDefaultAsync();
    }
    
    public IQueryable<Entities.Pharmacy> Query()
    {
        return _context.Pharmacies
            .Include(p => p.Address)
            .AsNoTracking()
            .AsQueryable();
    }
}
using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class ManufacturerRepository : BaseRepository, IManufacturerRepository
{
    private readonly PharmacyDbContext _context;

    public ManufacturerRepository(PharmacyDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Manufacturer>> GetAllAsync()
    {
        return await _context.Manufacturers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Manufacturer?> GetByIdAsync(int id)
    {
        return await _context.Manufacturers.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExistsAsync(int manufacturerId = 0, string? name = null, int? excludeId = null)
    {
        return await _context.Manufacturers.AnyAsync(x =>
            ((manufacturerId != 0 && x.Id == manufacturerId) || 
             (!string.IsNullOrEmpty(name) && x.Name.ToLower() == name.ToLower())) && 
            (!excludeId.HasValue || x.Id != excludeId));
    }

    public async Task AddAsync(Manufacturer manufacturer)
    {
        await _context.Manufacturers.AddAsync(manufacturer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Manufacturer manufacturer)
    {
        _context.Manufacturers.Update(manufacturer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Manufacturer manufacturer)
    {
        _context.Manufacturers.Remove(manufacturer);
        await _context.SaveChangesAsync();
    }
}
namespace Pharmacy.Database.Repositories.Interfaces;

public interface IPharmacyRepository
{
    Task AddAsync(Entities.Pharmacy pharmacy);
    Task<IEnumerable<Entities.Pharmacy>> GetAllAsync();
    Task<Entities.Pharmacy?> GetByIdAsync(int id);
    Task<Entities.Pharmacy?> GetByOsmAndCoordinatesAsync(string name, string? osmId, double latitude, double longitude);
    Task<bool> ExistsAsync(string name, string? osmId, double latitude, double longitude);
    Task<bool> ExistsByIdAsync(int pharmacyId);
    Task<Entities.Pharmacy?> GetNearestAsync(double latitude, double longitude);
    IQueryable<Entities.Pharmacy> Query();
}
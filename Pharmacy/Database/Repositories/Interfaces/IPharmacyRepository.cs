namespace Pharmacy.Database.Repositories.Interfaces;

public interface IPharmacyRepository
{
    Task AddAsync(Entities.Pharmacy pharmacy);
    Task<Entities.Pharmacy?> GetByOsmAndCoordinatesAsync(string name, string? osmId, double latitude, double longitude);
    Task<bool> ExistsAsync(string name, string? osmId, double latitude, double longitude);
    Task<bool> ExistsByIdAsync(int pharmacyId);
}
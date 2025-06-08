using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByOrderIdAsync(int orderId);
    Task AddAsync(Delivery delivery);
    Task UpdateAsync(Delivery delivery);
    IQueryable<Delivery> QueryWithDetails();
}
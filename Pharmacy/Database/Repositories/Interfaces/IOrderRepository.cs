using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<Order?> GetByIdWithDetailsAsync(int orderId, bool includeItems = true, bool includeStatus = true, bool includeProductImages = true, bool includePayment = false, bool includeUser = false, bool includePharmacy = false);
    Task<IEnumerable<OrderStatus>> GetAllOrderStatuses();
    IQueryable<Order> QueryWithStatus();
}
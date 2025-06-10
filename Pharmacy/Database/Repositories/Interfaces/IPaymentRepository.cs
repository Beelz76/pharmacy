using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByIdWithDetailsAsync(int id);
    IQueryable<Payment> QueryWithDetails();
}
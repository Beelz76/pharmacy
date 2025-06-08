using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IPaymentStatusRepository
{
    Task<IEnumerable<PaymentStatus>> GetAllAsync();
}
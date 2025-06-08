using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services.Interfaces;

public interface IPaymentStatusService
{
    Task<IEnumerable<PaymentStatusDto>> GetAllAsync();
}
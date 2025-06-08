using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services.Interfaces;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetAllAsync();
}
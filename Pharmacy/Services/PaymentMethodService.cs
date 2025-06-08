using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;

    public PaymentMethodService(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync()
    {
        var methods = await _repository.GetAllAsync();
        return methods.Select(m => new PaymentMethodDto(m.Id, m.Name, m.Description));
    }
}
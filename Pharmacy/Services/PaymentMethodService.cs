using Microsoft.Extensions.Caching.Hybrid;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;
    private readonly HybridCache _cache;
    
    public PaymentMethodService(IPaymentMethodRepository repository, HybridCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync()
    {
        var methods = await _cache.GetOrCreateAsync(
            "payment-methods-all",
            async ct =>
            {
                var res = await _repository.GetAllAsync();
                return res.Select(m => new PaymentMethodDto(m.Id, m.Name, m.Description)).ToList();
            });

        return methods;
    }
}
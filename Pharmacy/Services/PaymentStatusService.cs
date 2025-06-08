using Microsoft.Extensions.Caching.Hybrid;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services;

public class PaymentStatusService : IPaymentStatusService
{
    private readonly IPaymentStatusRepository _repository;
    private readonly HybridCache _cache;
    
    public PaymentStatusService(IPaymentStatusRepository repository, HybridCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<PaymentStatusDto>> GetAllAsync()
    {
        var statuses = await _cache.GetOrCreateAsync(
            "payment-statuses-all",
            async ct =>
            {
                var res = await _repository.GetAllAsync();
                return res.Select(s => new PaymentStatusDto(s.Id, s.Name, s.Description)).ToList();
            });

        return statuses;
    }
}
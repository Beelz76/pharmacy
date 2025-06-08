using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Services;

public class PaymentStatusService : IPaymentStatusService
{
    private readonly IPaymentStatusRepository _repository;

    public PaymentStatusService(IPaymentStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentStatusDto>> GetAllAsync()
    {
        var statuses = await _repository.GetAllAsync();
        return statuses.Select(s => new PaymentStatusDto(s.Id, s.Name, s.Description));
    }
}
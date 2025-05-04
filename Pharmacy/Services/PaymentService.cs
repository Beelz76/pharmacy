using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PaymentService(IPaymentRepository paymentRepository, IDateTimeProvider dateTimeProvider)
    {
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task CreateInitialPaymentAsync(int orderId, decimal amount, PaymentMethodEnum method)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethodId = (int)method,
            StatusId = method == PaymentMethodEnum.Online ? (int)PaymentStatusEnum.Pending : (int)PaymentStatusEnum.NotPaid,
            Number = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
    }
    
    public async Task<Result> UpdateStatusAsync(int orderId, PaymentStatusEnum newStatus)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment is null)
        {
            return Result.Failure(Error.NotFound("Платёж не найден"));
        }

        payment.StatusId = (int)newStatus;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;
        if (newStatus == PaymentStatusEnum.Completed)
        {
            payment.TransactionDate = _dateTimeProvider.UtcNow;
        }

        await _paymentRepository.UpdateAsync(payment);
        return Result.Success();
    }

}
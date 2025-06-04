using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IPaymentService
{
    Task CreateInitialPaymentAsync(int orderId, decimal amount, PaymentMethodEnum method);
    Task<Result> UpdateStatusAsync(int orderId, PaymentStatusEnum newStatus);
    Task<Result> SetExternalPaymentIdAsync(int orderId, string externalPaymentId);
}
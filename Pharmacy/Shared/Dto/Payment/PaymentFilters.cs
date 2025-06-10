using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.Payment;

public record PaymentFilters(
    string? OrderNumber,
    int? PharmacyId,
    PaymentStatusEnum? Status,
    PaymentMethodEnum? Method,
    string? ExternalPaymentId,
    DateTime? FromDate,
    DateTime? ToDate);
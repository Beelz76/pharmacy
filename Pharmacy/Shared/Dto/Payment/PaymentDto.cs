namespace Pharmacy.Shared.Dto.Payment;

public record PaymentDto(
    int Id,
    string Method,
    decimal Amount,
    string Status,
    DateTime? TransactionDate,
    string? ExternalPaymentId
);
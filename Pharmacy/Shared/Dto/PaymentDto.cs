namespace Pharmacy.Shared.Dto;

public record PaymentDto(
    int Id,
    string Method,
    decimal Amount,
    string Status,
    DateTime? TransactionDate
);
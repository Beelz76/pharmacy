namespace Pharmacy.Shared.Dto.Payment;

public record PaymentDetailsDto(
    int Id,
    int OrderId,
    string OrderNumber,
    int PharmacyId,
    string PharmacyName,
    string PharmacyAddress,
    decimal Amount,
    string Method,
    string Status,
    DateTime? TransactionDate);
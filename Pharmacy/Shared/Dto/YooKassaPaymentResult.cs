namespace Pharmacy.Shared.Dto;

public class YooKassaPaymentResult
{
    public string Id { get; set; } = null!;
    public YooKassaConfirmationResult Confirmation { get; set; } = null!;

    public string ConfirmationUrl => Confirmation?.ConfirmationUrl ?? string.Empty;
}

public class YooKassaConfirmationResult
{
    public string Type { get; set; } = null!;
    public string ConfirmationUrl { get; set; } = null!;
}

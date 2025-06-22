namespace Pharmacy.Shared.Dto.YooKassa;

public class YooKassaPaymentResult
{
    public string Id { get; set; } = null!;
    public YooKassaConfirmationResult Confirmation { get; set; } = null!;
}

public class YooKassaConfirmationResult
{
    public string Type { get; set; } = null!;
    public string ConfirmationUrl { get; set; } = null!;
}

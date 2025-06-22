namespace Pharmacy.Shared.Dto.YooKassa;

public class YooKassaCreatePaymentRequest
{
    public YooKassaAmount Amount { get; set; } = null!;
    public string Description { get; set; } = null!;
    public YooKassaConfirmation Confirmation { get; set; } = null!;
    public bool Capture { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class YooKassaAmount
{
    public decimal Value { get; set; }
    public string Currency { get; set; } = "RUB";
}

public class YooKassaConfirmation
{
    public string Type { get; set; } = "redirect";
    public string ReturnUrl { get; set; } = null!;
}

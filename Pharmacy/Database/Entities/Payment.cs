namespace Pharmacy.Database.Entities;

public class Payment
{
    public int Id { get; set; }
    public string? Number { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public int StatusId { get; set; }
    public int PaymentMethodId { get; set; }
    public string? Comment { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
        
    public Order Order { get; set; } = default!;
    public PaymentStatus Status { get; set; } = default!;
    public PaymentMethod Method { get; set; } = default!;
}
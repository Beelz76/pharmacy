namespace Pharmacy.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public int StatusId { get; set; }
    public int PaymentMethodId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
        
    public Order Order { get; set; } = default!;
    public PaymentStatus PaymentStatus { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; } = default!;
}
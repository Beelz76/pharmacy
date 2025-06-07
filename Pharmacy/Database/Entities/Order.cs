namespace Pharmacy.Database.Entities;

public class Order
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public int StatusId { get; set; }
    public string? PickupCode { get; set; }
    public bool IsDelivery { get; set; }
    
    public int PharmacyId { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public User User { get; set; } = null!;
    public OrderStatus Status { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
    public Pharmacy? Pharmacy { get; set; }
    public Delivery? Delivery { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
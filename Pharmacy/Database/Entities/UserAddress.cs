namespace Pharmacy.Database.Entities;

public class UserAddress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public string? Apartment { get; set; }
    public string? Entrance { get; set; }
    public string? Floor { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public User User { get; set; } = null!;
    public Address Address { get; set; } = null!;

    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
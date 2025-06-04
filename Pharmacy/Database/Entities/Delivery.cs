namespace Pharmacy.Database.Entities;

public class Delivery
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int UserAddressId { get; set; }
    public string? Comment { get; set; }
    
    public DateTime? DeliveryDate { get; set; }

    public Order Order { get; set; } = null!;
    public UserAddress UserAddress { get; set; } = null!;
}
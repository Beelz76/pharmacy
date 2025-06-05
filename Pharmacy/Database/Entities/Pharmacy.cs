namespace Pharmacy.Database.Entities;

public class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public int  AddressId { get; set; }
    
    public Address Address { get; set; } = null!;

    public ICollection<PharmacyProduct> PharmacyProducts  { get; set; } = new List<PharmacyProduct>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
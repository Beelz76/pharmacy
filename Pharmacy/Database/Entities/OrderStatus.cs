namespace Pharmacy.Database.Entities;

public class OrderStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
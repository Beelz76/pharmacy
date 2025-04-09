namespace Pharmacy.Database.Entities;

public class OrderStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
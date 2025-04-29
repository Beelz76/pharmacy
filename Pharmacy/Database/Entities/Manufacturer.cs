namespace Pharmacy.Database.Entities;

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Country { get; set; } = default!;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
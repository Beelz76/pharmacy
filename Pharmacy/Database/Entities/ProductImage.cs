namespace Pharmacy.Database.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    
    public Product Product { get; set; } = default!;
}
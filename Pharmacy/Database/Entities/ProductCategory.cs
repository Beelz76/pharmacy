namespace Pharmacy.Database.Entities;

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductCategoryField> Fields { get; set; } = new List<ProductCategoryField>();
}
namespace Pharmacy.Database.Entities;

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int? ParentCategoryId { get; set; }
    
    public ProductCategory? ParentCategory { get; set; }
    
    public ICollection<ProductCategory> Subcategories { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductCategoryField> Fields { get; set; } = new List<ProductCategoryField>();
}
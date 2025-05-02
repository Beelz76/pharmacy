namespace Pharmacy.Database.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public int ManufacturerId { get; set; }
    public string Description { get; set; } = default!;
    public bool IsAvailable { get; set; }
    public bool IsPrescriptionRequired { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductCategory ProductCategory { get; set; } = default!;
    public Manufacturer Manufacturer { get; set; } = default!;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<FavoriteItem> Favorites { get; set; } = new List<FavoriteItem>();
    public ICollection<CartItem> Carts { get; set; } = new List<CartItem>();
    public ICollection<ProductProperty> Properties { get; set; } = new List<ProductProperty>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
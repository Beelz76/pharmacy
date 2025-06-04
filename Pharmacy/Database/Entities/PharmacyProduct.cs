namespace Pharmacy.Database.Entities;

public class PharmacyProduct
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public int ProductId { get; set; }
    public int StockQuantity { get; set; }
    public decimal? LocalPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    public DateTime? LastRestockedAt { get; set; }
    
    public Pharmacy Pharmacy { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
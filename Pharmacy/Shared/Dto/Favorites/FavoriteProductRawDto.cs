namespace Pharmacy.Shared.Dto.Favorites;

public class FavoriteProductRawDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ManufacturerName { get; set; }
    public string ManufacturerCountry { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
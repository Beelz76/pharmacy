namespace Pharmacy.Database.Entities;

public class ProductProperty
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;

    public Product Product { get; set; } = default!;
}
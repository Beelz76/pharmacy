namespace Pharmacy.Database.Entities;

public class ProductCategoryField
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string FieldKey { get; set; } = default!;
    public string FieldLabel { get; set; } = default!;
    public string FieldType { get; set; } = default!;
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }

    public ProductCategory ProductCategory { get; set; } = default!;
}
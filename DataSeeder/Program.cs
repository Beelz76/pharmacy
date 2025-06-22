using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataSeeder;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.Database.Entities;

var optionsBuilder = new DbContextOptionsBuilder<PharmacyDbContext>();
optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("Server=localhost;Port=5432;Database=PharmacyDb;Username=postgres;Password=6969;") ?? "Server=localhost;Port=5432;Database=PharmacyDb;Username=postgres;Password=6969;");

using var context = new PharmacyDbContext(optionsBuilder.Options);

var json = await File.ReadAllTextAsync("C:\\Users\\fario\\RiderProjects\\Pharmacy\\DataSeeder\\SeedData\\initial_data.json");
var data = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
}) ?? throw new Exception("No data");


await context.Database.MigrateAsync();

foreach (var m in data.Manufacturers)
{
    if (!context.Manufacturers.Any(x => x.Name == m.Name))
    {
        context.Manufacturers.Add(new Manufacturer { Name = m.Name, Country = m.Country });
    }
}
await context.SaveChangesAsync();

var categories = new Dictionary<string, ProductCategory>();
foreach (var c in data.Categories)
{
    var category = await EnsureCategoryAsync(c, null);
    categories[c.Name] = category;
    if (c.Subcategories != null)
    {
        foreach (var sc in c.Subcategories)
        {
            var sub = await EnsureCategoryAsync(sc, category.Id);
            categories[sc.Name] = sub;
        }
    }
}

await context.SaveChangesAsync();

foreach (var prod in data.Products)
{
    var manufacturer = context.Manufacturers.First(x => x.Name == prod.Manufacturer);
    var category = categories[prod.Subcategory];

    if (!context.Products.Any(x => x.Sku == prod.Sku))
    {
        var p = new Product
        {
            Sku = prod.Sku,
            Name = prod.Name,
            Price = prod.Price,
            CategoryId = category.Id,
            ManufacturerId = manufacturer.Id,
            Description = prod.Description,
            ExtendedDescription = prod.ExtendedDescription,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsGloballyDisabled = false
        };
        foreach (var (key, value) in prod.Properties)
        {
            p.Properties.Add(new ProductProperty { Key = key, Value = value });
        }
        context.Products.Add(p);
    }
}

await context.SaveChangesAsync();

Console.WriteLine("Seed data inserted");

async Task<ProductCategory> EnsureCategoryAsync(CategoryDto dto, int? parentId)
{
    var existing = await context.ProductCategories.FirstOrDefaultAsync(x => x.Name == dto.Name);
    if (existing == null)
    {
        existing = new ProductCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = parentId
        };
        context.ProductCategories.Add(existing);
        await context.SaveChangesAsync();

        if (dto.Fields != null)
        {
            foreach (var field in dto.Fields)
            {
                existing.Fields.Add(new ProductCategoryField
                {
                    FieldKey = field.Key,
                    FieldLabel = field.Label,
                    FieldType = field.Type,
                    IsRequired = field.Required,
                    IsFilterable = field.Filterable
                });
            }
        }
        await context.SaveChangesAsync();
    }
    return existing;
}

namespace DataSeeder
{
    record SeedData(List<ManufacturerDto> Manufacturers, List<CategoryDto> Categories, List<ProductDto> Products);
    record ManufacturerDto(string Name, string Country);
    record CategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FieldDto>? Fields { get; set; }
        public List<CategoryDto>? Subcategories { get; set; }
    }
    record FieldDto(string Key, string Label, string Type, bool Required, bool Filterable);
    record ProductDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ExtendedDescription { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
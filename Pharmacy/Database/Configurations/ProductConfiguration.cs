using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.StockQuantity)
            .IsRequired();

        builder.Property(x => x.ExpirationDate);
        
        builder.Property(x => x.IsAvailable)
            .IsRequired();
        
        builder.Property(x => x.IsPrescriptionRequired)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manufacturer)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Properties)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => x.Name);
    }
}
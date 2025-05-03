using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("Ref_ProductCategories");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasMany(x => x.Products)
            .WithOne(x => x.ProductCategory)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Fields)
            .WithOne(x => x.ProductCategory)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(pc => pc.Subcategories)
            .WithOne(pc => pc.ParentCategory)
            .HasForeignKey(pc => pc.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.ParentCategoryId);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
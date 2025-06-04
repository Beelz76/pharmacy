using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class ProductCategoryFieldConfiguration : IEntityTypeConfiguration<ProductCategoryField>
{
    public void Configure(EntityTypeBuilder<ProductCategoryField> builder)
    {
        builder.ToTable("ProductCategoryFields");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.FieldKey)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.FieldLabel)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.FieldType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.IsFilterable)
            .IsRequired();

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
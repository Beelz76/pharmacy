using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class ProductPropertyConfiguration : IEntityTypeConfiguration<ProductProperty>
{
    public void Configure(EntityTypeBuilder<ProductProperty> builder)
    {
        builder.ToTable("ProductProperties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Properties)
            .HasForeignKey(x => x.ProductId);
        
        builder.HasIndex(x => new { x.Key, x.Value });
        builder.HasIndex(x => x.ProductId);
        
    }
}
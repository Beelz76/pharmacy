using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class PharmacyProductConfiguration : IEntityTypeConfiguration<PharmacyProduct>
{
    public void Configure(EntityTypeBuilder<PharmacyProduct> builder)
    {
        builder.ToTable("PharmacyProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StockQuantity)
            .IsRequired();
        
        builder.Property(x => x.LocalPrice)
            .HasPrecision(18, 2);
        
        builder.Property(x => x.IsAvailable)
            .IsRequired();
        
        builder.Property(x => x.LastRestockedAt);

        builder.HasOne(x => x.Pharmacy)
            .WithMany(x => x.PharmacyProducts)
            .HasForeignKey(x => x.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.PharmacyProducts)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
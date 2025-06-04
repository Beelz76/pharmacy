using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pharmacy.Database.Configurations;

public class PharmacyConfiguration : IEntityTypeConfiguration<Entities.Pharmacy>
{
    public void Configure(EntityTypeBuilder<Entities.Pharmacy> builder)
    {
        builder.ToTable("Pharmacies");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.Phone)
            .HasMaxLength(20);
        
        builder.Property(x => x.IsActive)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasOne(x => x.Address)
            .WithMany()
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PharmacyProducts)
            .WithOne(x => x.Pharmacy)
            .HasForeignKey(x => x.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.Pharmacy)
            .HasForeignKey(x => x.PharmacyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
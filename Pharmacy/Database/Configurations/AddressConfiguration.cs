using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.OsmId)
            .HasMaxLength(100);

        builder.Property(a => a.Region)
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .HasMaxLength(100);

        builder.Property(a => a.Suburb)
            .HasMaxLength(100);

        builder.Property(a => a.Street)
            .HasMaxLength(150);

        builder.Property(a => a.HouseNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Postcode)
            .HasMaxLength(20);

        builder.Property(a => a.Latitude)
            .IsRequired();

        builder.Property(a => a.Longitude)
            .IsRequired();
    }
}
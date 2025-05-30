﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;
using Pharmacy.Extensions;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("Ref_PaymentMethods");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Method)
            .HasForeignKey(x => x.PaymentMethodId);
        
        builder.HasData(EnumExtensions.GetEnumDetails<PaymentMethodEnum>()
            .Select(x => new PaymentMethod
            {
                Id = (int)x.Item1,
                Name = x.Item1.ToString(),
                Description = x.Item2
            }).ToArray());
    }
}
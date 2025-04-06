using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Models.Entities;

namespace Pharmacy.Data.Configurations;

public class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable("EmailVerificationCodes");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.IsUsed)
            .IsRequired();
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.EmailVerificationCodes)
            .HasForeignKey(x => x.UserId);
    }
}
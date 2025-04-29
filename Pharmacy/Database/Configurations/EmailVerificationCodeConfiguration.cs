using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable("EmailVerificationCodes");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(x => x.ExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.IsUsed)
            .IsRequired();
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.EmailVerificationCodes)
            .HasForeignKey(x => x.UserId);
        
        builder.HasIndex(x => new { x.Email, x.Code, x.Purpose });
        builder.HasIndex(x => x.ExpiresAt);
    }
}
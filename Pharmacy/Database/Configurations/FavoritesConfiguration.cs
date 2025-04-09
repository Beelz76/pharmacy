using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class FavoritesConfiguration : IEntityTypeConfiguration<FavoriteItem>
{
    public void Configure(EntityTypeBuilder<FavoriteItem> builder)
    {
        builder.ToTable("FavoriteItems");

        builder.HasKey(x => new {x.UserId, x.ProductId});
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.FavoriteItems)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.ProductId);
    }
}
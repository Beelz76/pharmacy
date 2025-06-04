using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.Database.Entities;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class ProductImageService : IProductImageService
{
    private readonly PharmacyDbContext _context;
    private readonly IStorageProvider _storage;

    public ProductImageService(PharmacyDbContext context, IStorageProvider storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result<List<string>>> UploadImagesAsync(int productId, IReadOnlyList<IFormFile> files)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Result.Failure<List<string>>(Error.NotFound("Товар не найден"));
        }

        var urls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var ext = Path.GetExtension(file.FileName);
            var key = $"products/{Guid.NewGuid()}{ext}";

            await using var stream = file.OpenReadStream();
            await _storage.UploadAsync(key, stream, file.ContentType);

            _context.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                Url = key,
                CreatedAt = DateTime.UtcNow
            });

            urls.Add(_storage.GetPublicUrl(key));
        }

        await _context.SaveChangesAsync();

        return Result.Success(urls);
    }

    public async Task<Result> DeleteProductImagesAsync(int productId, List<int> imageIds)
    {
        var images = await _context.ProductImages
            .Where(x => x.ProductId == productId && imageIds.Contains(x.Id))
            .ToListAsync();

        if (!images.Any())
        {
            return Result.Failure(Error.NotFound("Изображения не найдены"));
        }

        foreach (var image in images)
        {
            if (!image.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                await _storage.DeleteAsync(image.Url);
            }
        }

        _context.ProductImages.RemoveRange(images);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> AddExternalImagesAsync(int productId, List<string> imageUrls)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Result.Failure(Error.NotFound("Товар не найден"));
        }

        var now = DateTime.UtcNow;

        foreach (var imageUrl in imageUrls)
        {
            _context.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                Url = imageUrl,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

}

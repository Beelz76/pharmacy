using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.Database.Entities;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Product;
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

    public async Task<Result<List<ProductImageDto>>> UploadImagesAsync(int productId, IReadOnlyList<IFormFile> files)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Result.Failure<List<ProductImageDto>>(Error.NotFound("Товар не найден"));
        }

        var result = new List<ProductImageDto>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var ext = Path.GetExtension(file.FileName);
            var key = $"products/{Guid.NewGuid()}{ext}";

            await using var stream = file.OpenReadStream();
            await _storage.UploadAsync(key, stream, file.ContentType);

            var image = new ProductImage
            {
                ProductId = productId,
                Url = key,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductImages.Add(image);
            result.Add(new ProductImageDto(image.Id, _storage.GetPublicUrl(key)));
        }

        await _context.SaveChangesAsync();

        return Result.Success(result);
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
    
    public async Task<Result<List<ProductImageDto>>> AddExternalImagesAsync(int productId, List<string> imageUrls)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Result.Failure<List<ProductImageDto>>(Error.NotFound("Товар не найден"));
        }

        var now = DateTime.UtcNow;

        var dtos = new List<ProductImageDto>();
        foreach (var imageUrl in imageUrls)
        {
            var entity = new ProductImage
            {
                ProductId = productId,
                Url = imageUrl,
                CreatedAt = now
            };
            _context.ProductImages.Add(entity);
            dtos.Add(new ProductImageDto(entity.Id, imageUrl));
        }

        await _context.SaveChangesAsync();
        return Result.Success(dtos);
    }

}

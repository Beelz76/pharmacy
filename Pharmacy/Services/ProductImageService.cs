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

    public async Task<Result> DeleteProductImageAsync(int productId, int imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);

        if (image is null)
        {
            return Result.Failure(Error.NotFound("Изображение не найдено"));
        }

        await _storage.DeleteAsync(image.Url);
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}

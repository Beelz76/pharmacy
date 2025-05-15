using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductImageService
{
    Task<Result<List<string>>> UploadImagesAsync(int productId, IReadOnlyList<IFormFile> files);
    Task<Result> DeleteProductImageAsync(int productId, int imageId);
}
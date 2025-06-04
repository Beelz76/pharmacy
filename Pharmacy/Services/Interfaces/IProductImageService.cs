using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductImageService
{
    Task<Result<List<string>>> UploadImagesAsync(int productId, IReadOnlyList<IFormFile> files);
    Task<Result> DeleteProductImagesAsync(int productId, List<int> imageIds);
    Task<Result> AddExternalImagesAsync(int productId, List<string> imageUrls);
}
using Pharmacy.Shared.Dto.Product;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductImageService
{
    Task<Result<List<ProductImageDto>>> UploadImagesAsync(int productId, IReadOnlyList<IFormFile> files);
    Task<Result> DeleteProductImagesAsync(int productId, List<int> imageIds);
    Task<Result<List<ProductImageDto>>> AddExternalImagesAsync(int productId, List<string> imageUrls);
}
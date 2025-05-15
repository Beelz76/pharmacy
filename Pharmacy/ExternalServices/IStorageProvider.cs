namespace Pharmacy.ExternalServices;

public interface IStorageProvider
{
    Task UploadAsync(string path, Stream stream, string contentType);
    Task DeleteAsync(string path);
    string? GetPublicUrl(string? path);
}
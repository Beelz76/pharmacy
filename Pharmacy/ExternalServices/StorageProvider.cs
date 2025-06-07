using Minio;
using Minio.DataModel.Args;

namespace Pharmacy.ExternalServices;

public class StorageProvider : IStorageProvider
{
    private readonly IMinioClient _minio;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;

    public StorageProvider(IConfiguration config, IMinioClient minio)
    {
        _minio = minio;
        _bucket = config["S3:Bucket"] ?? string.Empty;
        _publicBaseUrl = config["S3:PublicBaseUrl"] ?? string.Empty;
    }

    public async Task UploadAsync(string path, Stream stream, string contentType)
    {
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(path)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType));
    }

    public async Task DeleteAsync(string path)
    {
        await _minio.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(path));
    }

    public string? GetPublicUrl(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? null : $"{_publicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

}
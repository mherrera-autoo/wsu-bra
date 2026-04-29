namespace ERP.Documents.Contracts.Storage;

public interface IBinaryStorage
{
    Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct);
    Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct);
    Task DeleteAsync(string storageKey, CancellationToken ct);
}

public sealed record BinaryStorageUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? StorageKey = null);

public sealed record BinaryStorageUploadResult(
    string StorageKey,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record BinaryStorageDownloadResult(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    IReadOnlyDictionary<string, string> Metadata);

public sealed class BinaryStorageOptions
{
    public BinaryStorageProvider Provider { get; set; } = BinaryStorageProvider.S3;
    public S3StorageOptions S3 { get; set; } = new();
    public AzureBlobStorageOptions Azure { get; set; } = new();
    public MinioStorageOptions Minio { get; set; } = new();
    public LocalStorageOptions Local { get; set; } = new();
}

public enum BinaryStorageProvider
{
    S3 = 1,
    Azure = 2,
    Minio = 3,
    Local = 4
}

public sealed class S3StorageOptions
{
    public string BucketName { get; set; } = "";
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}

public sealed class AzureBlobStorageOptions
{
    public string ConnectionString { get; set; } = "";
    public string ContainerName { get; set; } = "";
}

public sealed class MinioStorageOptions
{
    public string Endpoint { get; set; } = "";
    public string BucketName { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool UseSsl { get; set; } = true;
}

public sealed class LocalStorageOptions
{
    public string BasePath { get; set; } = "";
}

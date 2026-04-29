using ERP.Documents.Contracts.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Documents.Infrastructure.BinaryStorage;

public sealed class MinioBinaryStorage : IBinaryStorage
{
    private readonly IOptionsMonitor<BinaryStorageOptions> _options;
    private readonly ILogger<MinioBinaryStorage> _logger;

    public MinioBinaryStorage(IOptionsMonitor<BinaryStorageOptions> options, ILogger<MinioBinaryStorage> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("MinIO provider not configured yet. Upload skipped for {FileName}.", request.FileName);
        throw new NotImplementedException("MinIO binary storage provider not configured yet.");
    }

    public Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("MinIO provider not configured yet. Download skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("MinIO binary storage provider not configured yet.");
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("MinIO provider not configured yet. Delete skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("MinIO binary storage provider not configured yet.");
    }

    private void EnsureConfigured()
    {
        var options = _options.CurrentValue.Minio;
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("MinIO endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("MinIO bucket name is not configured.");
        }
    }
}

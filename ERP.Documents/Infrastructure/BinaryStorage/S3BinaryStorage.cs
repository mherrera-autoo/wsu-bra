using ERP.Documents.Contracts.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Documents.Infrastructure.BinaryStorage;

public sealed class S3BinaryStorage : IBinaryStorage
{
    private readonly IOptionsMonitor<BinaryStorageOptions> _options;
    private readonly ILogger<S3BinaryStorage> _logger;

    public S3BinaryStorage(IOptionsMonitor<BinaryStorageOptions> options, ILogger<S3BinaryStorage> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("S3 provider not configured yet. Upload skipped for {FileName}.", request.FileName);
        throw new NotImplementedException("S3 binary storage provider not configured yet.");
    }

    public Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("S3 provider not configured yet. Download skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("S3 binary storage provider not configured yet.");
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("S3 provider not configured yet. Delete skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("S3 binary storage provider not configured yet.");
    }

    private void EnsureConfigured()
    {
        var options = _options.CurrentValue.S3;
        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("S3 bucket name is not configured.");
        }
    }
}

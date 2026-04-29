using ERP.Documents.Contracts.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Documents.Infrastructure.BinaryStorage;

public sealed class AzureBlobBinaryStorage : IBinaryStorage
{
    private readonly IOptionsMonitor<BinaryStorageOptions> _options;
    private readonly ILogger<AzureBlobBinaryStorage> _logger;

    public AzureBlobBinaryStorage(IOptionsMonitor<BinaryStorageOptions> options, ILogger<AzureBlobBinaryStorage> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("Azure blob provider not configured yet. Upload skipped for {FileName}.", request.FileName);
        throw new NotImplementedException("Azure blob storage provider not configured yet.");
    }

    public Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("Azure blob provider not configured yet. Download skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("Azure blob storage provider not configured yet.");
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        EnsureConfigured();
        _logger.LogWarning("Azure blob provider not configured yet. Delete skipped for {StorageKey}.", storageKey);
        throw new NotImplementedException("Azure blob storage provider not configured yet.");
    }

    private void EnsureConfigured()
    {
        var options = _options.CurrentValue.Azure;
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Azure blob connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.ContainerName))
        {
            throw new InvalidOperationException("Azure blob container name is not configured.");
        }
    }
}

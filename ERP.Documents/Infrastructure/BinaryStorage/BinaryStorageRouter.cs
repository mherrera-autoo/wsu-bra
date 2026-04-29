using ERP.Documents.Contracts.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Documents.Infrastructure.BinaryStorage;

public sealed class BinaryStorageRouter : IBinaryStorage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BinaryStorageRouter> _logger;
    private readonly IOptionsMonitor<BinaryStorageOptions> _options;
    private IBinaryStorage _storage;

    public BinaryStorageRouter(
        IServiceProvider serviceProvider,
        IOptionsMonitor<BinaryStorageOptions> options,
        ILogger<BinaryStorageRouter> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        _storage = CreateStorage(options.CurrentValue.Provider);

        _options.OnChange(newOptions =>
        {
            _logger.LogInformation("Binary storage provider switched to {Provider}.", newOptions.Provider);
            _storage = CreateStorage(newOptions.Provider);
        });
    }

    public Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct)
    {
        return _storage.UploadAsync(request, ct);
    }

    public Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct)
    {
        return _storage.DownloadAsync(storageKey, ct);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        return _storage.DeleteAsync(storageKey, ct);
    }

    private IBinaryStorage CreateStorage(BinaryStorageProvider provider)
    {
        return provider switch
        {
            BinaryStorageProvider.S3 => _serviceProvider.GetRequiredService<S3BinaryStorage>(),
            BinaryStorageProvider.Azure => _serviceProvider.GetRequiredService<AzureBlobBinaryStorage>(),
            BinaryStorageProvider.Minio => _serviceProvider.GetRequiredService<MinioBinaryStorage>(),
            BinaryStorageProvider.Local => _serviceProvider.GetRequiredService<LocalBinaryStorage>(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported storage provider.")
        };
    }
}

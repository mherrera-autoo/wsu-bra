using System.Text.Json;
using ERP.Documents.Contracts.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Documents.Infrastructure.BinaryStorage;

public sealed class LocalBinaryStorage : IBinaryStorage
{
    private const string MetadataExtension = ".meta.json";
    private readonly IOptionsMonitor<BinaryStorageOptions> _options;
    private readonly ILogger<LocalBinaryStorage> _logger;

    public LocalBinaryStorage(IOptionsMonitor<BinaryStorageOptions> options, ILogger<LocalBinaryStorage> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<BinaryStorageUploadResult> UploadAsync(BinaryStorageUploadRequest request, CancellationToken ct)
    {
        var basePath = EnsureConfigured();
        var storageKey = request.StorageKey ?? $"{Guid.NewGuid():N}";
        var safeKey = EnsureSafeKey(storageKey);
        var filePath = Path.Combine(basePath, safeKey);

        Directory.CreateDirectory(basePath);

        await using (var targetStream = File.Create(filePath))
        {
            await request.Content.CopyToAsync(targetStream, ct);
        }

        var metadata = new LocalBinaryStorageMetadata(
            request.FileName,
            request.ContentType,
            request.SizeBytes,
            request.Metadata ?? new Dictionary<string, string>());

        var metadataPath = Path.Combine(basePath, $"{safeKey}{MetadataExtension}");
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata), ct);

        _logger.LogInformation("Stored file {StorageKey} on local storage.", safeKey);

        return new BinaryStorageUploadResult(safeKey, metadata.Metadata);
    }

    public async Task<BinaryStorageDownloadResult> DownloadAsync(string storageKey, CancellationToken ct)
    {
        var basePath = EnsureConfigured();
        var safeKey = EnsureSafeKey(storageKey);
        var filePath = Path.Combine(basePath, safeKey);
        var metadataPath = Path.Combine(basePath, $"{safeKey}{MetadataExtension}");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Binary file not found.", filePath);
        }

        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException("Binary metadata not found.", metadataPath);
        }

        var metadataJson = await File.ReadAllTextAsync(metadataPath, ct);
        var metadata = JsonSerializer.Deserialize<LocalBinaryStorageMetadata>(metadataJson)
            ?? throw new InvalidOperationException("Binary metadata is invalid.");

        var content = File.OpenRead(filePath);
        return new BinaryStorageDownloadResult(
            content,
            metadata.FileName,
            metadata.ContentType,
            metadata.SizeBytes,
            metadata.Metadata);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        var basePath = EnsureConfigured();
        var safeKey = EnsureSafeKey(storageKey);
        var filePath = Path.Combine(basePath, safeKey);
        var metadataPath = Path.Combine(basePath, $"{safeKey}{MetadataExtension}");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }

        _logger.LogInformation("Deleted file {StorageKey} from local storage.", safeKey);

        return Task.CompletedTask;
    }

    private string EnsureConfigured()
    {
        var options = _options.CurrentValue.Local;
        if (string.IsNullOrWhiteSpace(options.BasePath))
        {
            throw new InvalidOperationException("Local storage base path is not configured.");
        }

        return options.BasePath;
    }

    private static string EnsureSafeKey(string storageKey)
    {
        var safeKey = Path.GetFileName(storageKey);
        if (!string.Equals(storageKey, safeKey, StringComparison.Ordinal))
        {
            throw new ArgumentException("Storage key contains invalid path characters.", nameof(storageKey));
        }

        return safeKey;
    }

    private sealed record LocalBinaryStorageMetadata(
        string FileName,
        string ContentType,
        long SizeBytes,
        IReadOnlyDictionary<string, string> Metadata);
}

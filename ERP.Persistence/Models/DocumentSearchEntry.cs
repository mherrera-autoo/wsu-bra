using System;
using System.Collections.Generic;
using ERP.Shared.Domain;

namespace ERP.Persistence;

public sealed class DocumentSearchEntry : CompanyEntity
{
    private DocumentSearchEntry()
    {
    }

    public string DocumentType { get; private set; } = null!;
    public string DocumentId { get; private set; } = null!;
    public string? Title { get; private set; }
    public string Content { get; private set; } = null!;
    public Dictionary<string, string> Metadata { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public static DocumentSearchEntry Create(
        long companyId,
        string documentType,
        string documentId,
        string? title,
        string content,
        IReadOnlyDictionary<string, string>? metadata)
    {
        return new DocumentSearchEntry
        {
            CompanyId = companyId,
            DocumentType = documentType.Trim(),
            DocumentId = documentId.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            Content = content.Trim(),
            Metadata = metadata is null ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? title, string content, IReadOnlyDictionary<string, string>? metadata)
    {
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        Content = content.Trim();
        Metadata = metadata is null ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
        UpdatedAt = DateTime.UtcNow;
    }
}

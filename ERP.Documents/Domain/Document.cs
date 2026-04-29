using System.Collections.ObjectModel;

namespace ERP.Documents.Domain;

public sealed class Document
{
    private readonly List<DocumentVersion> _versions = new();
    private readonly List<DocumentLink> _links = new();
    private readonly List<string> _tags = new();

    private Document()
    {
        Title = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    private Document(Guid companyPublicId, string title, string? description, IEnumerable<string> tags)
        : this()
    {
        CompanyPublicId = companyPublicId;
        Title = title;
        Description = description;
        _tags.AddRange(tags);
    }

    public long Id { get; private set; }
    public Guid CompanyPublicId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<string> Tags => new ReadOnlyCollection<string>(_tags);
    public IReadOnlyList<DocumentVersion> Versions => new ReadOnlyCollection<DocumentVersion>(_versions);
    public IReadOnlyList<DocumentLink> Links => new ReadOnlyCollection<DocumentLink>(_links);

    public static Document Create(
        Guid companyPublicId,
        string title,
        string? description,
        IEnumerable<string> tags,
        string fileName,
        string contentType,
        long size,
        string? label)
    {
        var document = new Document(companyPublicId, title, description, tags);
        document.AddVersion(fileName, contentType, size, label, DateTime.UtcNow);
        return document;
    }

    public void AssignId(long id)
    {
        Id = id;
    }

    public DocumentVersion AddVersion(string fileName, string contentType, long size, string? label, DateTime uploadedAt)
    {
        var version = new DocumentVersion(0, CompanyPublicId, fileName, contentType, size, label, uploadedAt);
        _versions.Add(version);
        return version;
    }

    public DocumentLink AddLink(string url, string? description, DateTime createdAt)
    {
        var link = new DocumentLink(0, CompanyPublicId, url, description, createdAt);
        _links.Add(link);
        return link;
    }
}

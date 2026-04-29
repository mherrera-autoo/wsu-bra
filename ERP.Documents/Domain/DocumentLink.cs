namespace ERP.Documents.Domain;

public sealed class DocumentLink
{
    public DocumentLink(long id, Guid companyPublicId, string url, string? description, DateTime createdAt)
    {
        Id = id;
        CompanyPublicId = companyPublicId;
        Url = url;
        Description = description;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public Guid CompanyPublicId { get; private set; }
    public string Url { get; }
    public string? Description { get; }
    public DateTime CreatedAt { get; }

    internal void AssignId(long id)
    {
        Id = id;
    }
}

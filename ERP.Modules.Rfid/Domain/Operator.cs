namespace ERP.Modules.Rfid.Domain;

public sealed class Operator : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string Code { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? DocumentId { get; private set; }
    public bool IsActive { get; private set; }

    private Operator() { }

    public static Operator Create(Guid companyPublicId, string code, string fullName, string? documentId, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Operator code is required.", nameof(code));
        }

        return new Operator
        {
            CompanyPublicId = companyPublicId,
            Code = code.Trim(),
            FullName = fullName.Trim(),
            DocumentId = string.IsNullOrWhiteSpace(documentId) ? null : documentId.Trim(),
            IsActive = isActive,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

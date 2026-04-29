using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public enum TaxDeclarationType
{
    MonthlyVat = 1,
    Withholding = 2,
    AnnualIncome = 3
}

public enum TaxDeclarationStatus
{
    Draft = 1,
    Submitted = 2,
    Accepted = 3,
    Rejected = 4
}

public sealed class TaxDeclaration : CompanyEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public TaxDeclarationType DeclarationType { get; private set; }
    public TaxDeclarationStatus Status { get; private set; } = TaxDeclarationStatus.Draft;
    public string Payload { get; private set; } = string.Empty;
    public string? ExternalReference { get; private set; }
    public string? StatusMessage { get; private set; }

    private TaxDeclaration() { }

    public static TaxDeclaration Create(
        long companyId,
        int year,
        int month,
        TaxDeclarationType declarationType,
        string payload)
    {
        if (year <= 0) throw new ArgumentOutOfRangeException(nameof(year));
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("Payload is required.", nameof(payload));

        return new TaxDeclaration
        {
            CompanyId = companyId,
            Year = year,
            Month = month,
            DeclarationType = declarationType,
            Payload = payload.Trim()
        };
    }

    public void MarkSubmitted(string? externalReference = null, string? message = null)
    {
        Status = TaxDeclarationStatus.Submitted;
        ExternalReference = externalReference;
        StatusMessage = message;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAccepted(string? message = null)
    {
        Status = TaxDeclarationStatus.Accepted;
        StatusMessage = message;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRejected(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
        Status = TaxDeclarationStatus.Rejected;
        StatusMessage = message.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}

using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class TaxEntity : Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string TaxId { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }

    private TaxEntity() { }

    public static TaxEntity Create(Guid companyPublicId, string taxId, string? displayName)
        => new()
        {
            CompanyPublicId = companyPublicId,
            TaxId = taxId.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim()
        };

    public void UpdateDisplayName(string? displayName)
        => DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
}

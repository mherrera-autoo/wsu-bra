using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Company : Entity
{
    public long OrganizationId { get; private set; }
    public long TaxEntityId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Company() { }

    public static Company Create(long organizationId, long taxEntityId, string name)
        => new()
        {
            OrganizationId = organizationId,
            TaxEntityId = taxEntityId,
            Name = name.Trim()
        };

public void Rename(string name)
        => Name = name.Trim();

    public void UpdateTaxEntityId(long taxEntityId)
        => TaxEntityId = taxEntityId;
}

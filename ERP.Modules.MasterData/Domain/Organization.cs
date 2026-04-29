using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Organization : Entity
{
    public OrganizationType AccountType { get; private set; }
    public string? DisplayName { get; private set; }

    private Organization() { }

    public static Organization Create(OrganizationType accountType, string? displayName = null)
        => new()
        {
            AccountType = accountType,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim()
        };

    public void UpdateDisplayName(string? displayName)
        => DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
}

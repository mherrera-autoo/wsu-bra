using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class City : Entity
{
    public long CountryId { get; private set; }
    public Country? Country { get; private set; }
    public long? SubdivisionId { get; private set; }
    public Subdivision? Subdivision { get; private set; }
    public string Name { get; private set; } = null!;
    public string? OfficialCode { get; private set; }
    public string? PostalCode { get; private set; }
    public bool IsCapital { get; private set; }
    public bool IsActive { get; private set; } = true;

    private City() { }

    public static City Create(
        long countryId,
        long? subdivisionId,
        string name,
        string? officialCode,
        string? postalCode,
        bool isCapital,
        bool isActive)
    {
        return new City
        {
            CountryId = countryId,
            SubdivisionId = subdivisionId,
            Name = name.Trim(),
            OfficialCode = string.IsNullOrWhiteSpace(officialCode) ? null : officialCode.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim(),
            IsCapital = isCapital,
            IsActive = isActive
        };
    }

    public void Update(
        long? subdivisionId,
        string name,
        string? officialCode,
        string? postalCode,
        bool isCapital,
        bool isActive)
    {
        SubdivisionId = subdivisionId;
        Name = name.Trim();
        OfficialCode = string.IsNullOrWhiteSpace(officialCode) ? null : officialCode.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        IsCapital = isCapital;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}

using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Subdivision : Entity
{
    public long CountryId { get; private set; }
    public Country? Country { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public short Level { get; private set; }
    public long? ParentSubdivisionId { get; private set; }
    public Subdivision? ParentSubdivision { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Subdivision() { }

    public static Subdivision Create(
        long countryId,
        string code,
        string name,
        short level,
        long? parentSubdivisionId,
        bool isActive)
    {
        return new Subdivision
        {
            CountryId = countryId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Level = level,
            ParentSubdivisionId = parentSubdivisionId,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        short level,
        long? parentSubdivisionId,
        bool isActive)
    {
        Name = name.Trim();
        Level = level;
        ParentSubdivisionId = parentSubdivisionId;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}

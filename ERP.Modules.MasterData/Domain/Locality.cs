using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Locality : Entity
{
    public long CityId { get; private set; }
    public City? City { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private Locality() { }

    public static Locality Create(long cityId, string name, bool isActive)
    {
        return new Locality
        {
            CityId = cityId,
            Name = name.Trim(),
            IsActive = isActive
        };
    }

    public void Update(string name, bool isActive)
    {
        Name = name.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}

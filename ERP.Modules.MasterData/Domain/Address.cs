using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Address : Entity
{
    public long CountryId { get; private set; }
    public Country? Country { get; private set; }
    public long? Level1SubdivisionId { get; private set; }
    public Subdivision? Level1Subdivision { get; private set; }
    public long? Level2SubdivisionId { get; private set; }
    public Subdivision? Level2Subdivision { get; private set; }
    public long? CityId { get; private set; }
    public City? City { get; private set; }
    public long? LocalityId { get; private set; }
    public Locality? Locality { get; private set; }
    public string Street { get; private set; } = null!;
    public string? Number { get; private set; }
    public string? Unit { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Notes { get; private set; }
    public decimal? GeoLat { get; private set; }
    public decimal? GeoLng { get; private set; }

    private Address() { }

    public static Address Create(
        long countryId,
        long? level1SubdivisionId,
        long? level2SubdivisionId,
        long? cityId,
        long? localityId,
        string street,
        string? number,
        string? unit,
        string? postalCode,
        string? notes,
        decimal? geoLat,
        decimal? geoLng)
    {
        return new Address
        {
            CountryId = countryId,
            Level1SubdivisionId = level1SubdivisionId,
            Level2SubdivisionId = level2SubdivisionId,
            CityId = cityId,
            LocalityId = localityId,
            Street = street.Trim(),
            Number = string.IsNullOrWhiteSpace(number) ? null : number.Trim(),
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            GeoLat = geoLat,
            GeoLng = geoLng
        };
    }

    public void Update(
        long countryId,
        long? level1SubdivisionId,
        long? level2SubdivisionId,
        long? cityId,
        long? localityId,
        string street,
        string? number,
        string? unit,
        string? postalCode,
        string? notes,
        decimal? geoLat,
        decimal? geoLng)
    {
        CountryId = countryId;
        Level1SubdivisionId = level1SubdivisionId;
        Level2SubdivisionId = level2SubdivisionId;
        CityId = cityId;
        LocalityId = localityId;
        Street = street.Trim();
        Number = string.IsNullOrWhiteSpace(number) ? null : number.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        GeoLat = geoLat;
        GeoLng = geoLng;
        UpdatedAt = DateTime.UtcNow;
    }
}

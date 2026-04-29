namespace ERP.Api.Contracts.Geo;

public sealed record CountryOption(long Id, string Iso2, string Name);

public sealed record SubdivisionOption(long Id, string Code, string Name, short Level, long? ParentSubdivisionId);

public sealed record CityOption(long Id, string Name, string? OfficialCode, long? SubdivisionId);

public sealed record LocalityOption(long Id, string Name);

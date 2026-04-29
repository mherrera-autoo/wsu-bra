namespace ERP.Api.Contracts.Companies;

public sealed record ReplaceCompanyUnitsOfMeasureRequest(IReadOnlyCollection<long> UnitOfMeasureIds);

using System.Text.Json.Serialization;
using ERP.Modules.MasterData.Domain;

namespace ERP.Api.Contracts.Companies;

public sealed record CompanyUnitOfMeasureDefaultRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UnitOfMeasureDimension Dimension,
    long UnitId);

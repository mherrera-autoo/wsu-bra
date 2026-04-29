namespace ERP.Api.Configuration;

public sealed class WsuOptions
{
    public Guid? MasterCompanyPublicId { get; init; }
    public string? MasterCompanyName { get; init; } = "WE STOCK YOU SPA";
    public bool ValidarOperadorEnOrden { get; init; } = false;
}

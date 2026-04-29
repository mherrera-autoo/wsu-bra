namespace ERP.Shared.Application;

public sealed class MutableTenantContext : ITenantContext
{
    public string Scope { get; set; } = "system";
    public long UserId { get; set; }
    public Guid? CompanyPublicId { get; set; }
    public long? CompanyId { get; set; }
}

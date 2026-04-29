namespace ERP.Shared.Application;

public interface ITenantContext
{
    string Scope { get; }
    long UserId { get; }
    Guid? CompanyPublicId { get; }
    long? CompanyId { get; }
}

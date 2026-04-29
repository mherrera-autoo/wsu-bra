namespace ERP.Shared.Application;

public interface ICompanyContext
{
    long CompanyId { get; }
    Guid? CompanyPublicId { get; }
    long UserId { get; }
}

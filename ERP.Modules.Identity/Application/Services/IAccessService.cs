namespace ERP.Modules.Identity.Application.Services;

public interface IAccessService
{
    Task<bool> CanAccessCompanyAsync(long userId, Guid companyPublicId, CancellationToken cancellationToken = default);
}

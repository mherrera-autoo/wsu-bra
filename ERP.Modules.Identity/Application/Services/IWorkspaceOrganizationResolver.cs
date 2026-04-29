namespace ERP.Modules.Identity.Application.Services;

public interface IWorkspaceOrganizationResolver
{
    Task<long?> ResolveWorkspaceOrganizationIdAsync(long userId, long? organizationIdFromJwt, CancellationToken cancellationToken = default);
}
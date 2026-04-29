namespace ERP.Modules.Identity.Application.Services;

public interface IWorkspaceResolver
{
    Task<long> ResolveWorkspaceOrganizationIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> IsOrganizationMemberAsync(long userId, long organizationId, CancellationToken cancellationToken = default);
}

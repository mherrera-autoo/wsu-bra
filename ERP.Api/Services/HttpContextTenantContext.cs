using ERP.Shared.Application;

namespace ERP.Api.Services;

public sealed class HttpContextTenantContext : ITenantContext
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public HttpContextTenantContext(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public string Scope => _currentUserProvider.GetCurrentUser()?.Scope ?? string.Empty;
    public long UserId => _currentUserProvider.GetCurrentUser()?.UserId ?? 0;
    public Guid? CompanyPublicId => _currentUserProvider.GetCurrentUser()?.CompanyPublicId;
    public long? CompanyId => _currentUserProvider.GetCurrentUser()?.CompanyId;
}

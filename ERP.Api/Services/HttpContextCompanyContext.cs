using ERP.Shared.Application;

namespace ERP.Api.Services;

public sealed class HttpContextCompanyContext : ICompanyContext
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public HttpContextCompanyContext(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public long CompanyId => _currentUserProvider.GetCurrentUser()?.CompanyId ?? 0;

    public Guid? CompanyPublicId => _currentUserProvider.GetCurrentUser()?.CompanyPublicId;

    public long UserId => _currentUserProvider.GetCurrentUser()?.UserId ?? 0;
}

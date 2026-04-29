using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Security;

public interface ITokenService
{
    LoginResult CreateTenantToken(User user, long organizationId, Guid companyPublicId, long companyId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
    LoginResult CreatePlatformToken(User user, long organizationId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
}

using ERP.Modules.Identity.Domain;
using ERP.Modules.Identity.Contracts;

namespace ERP.Modules.Identity.Application.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Guid companyPublicId, string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<User?> GetByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserWithProfileSummary>> GetByPublicIdsWithProfileAsync(IReadOnlyCollection<Guid> userPublicIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserWithProfileSummary>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByCompanyAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Guid companyPublicId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(long userId, Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(long userId, Guid companyPublicId, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

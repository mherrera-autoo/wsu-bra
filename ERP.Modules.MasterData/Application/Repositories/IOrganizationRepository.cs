using ERP.Modules.MasterData.Contracts;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IOrganizationRepository
{
    Task<OrganizationSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<OrganizationSnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<OrganizationSnapshot> AddAsync(OrganizationCreateRequest organization, CancellationToken cancellationToken = default);
}

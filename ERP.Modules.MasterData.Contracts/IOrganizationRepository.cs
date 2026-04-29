namespace ERP.Modules.MasterData.Contracts;

public interface IOrganizationRepository
{
    Task<OrganizationSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<OrganizationSnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<OrganizationSnapshot> AddAsync(OrganizationCreateRequest organization, CancellationToken cancellationToken = default);
}

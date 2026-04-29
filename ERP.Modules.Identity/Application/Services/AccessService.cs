using System;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Domain;

namespace ERP.Modules.Identity.Application.Services;

public sealed class AccessService : IAccessService
{
    private readonly ICompanyUserRepository _companyUserRepository;

    public AccessService(
        ICompanyUserRepository companyUserRepository)
    {
        _companyUserRepository = companyUserRepository;
    }

    public async Task<bool> CanAccessCompanyAsync(long userId, Guid companyPublicId, CancellationToken cancellationToken = default)
    {
        if (companyPublicId == Guid.Empty || userId <= 0)
        {
            return false;
        }

        var directMembership = await _companyUserRepository.GetAsync(companyPublicId, userId, cancellationToken);
        return directMembership is not null && directMembership.Status == CompanyUserStatus.Active;
    }
}

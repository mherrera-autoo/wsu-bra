using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using Xunit;

namespace ERP.Modules.Identity.WhiteBox.Tests;

public sealed class AccessServiceTests
{
    [Fact]
    public async Task CanAccessCompanyAsync_ReturnsFalse_WhenCompanyUserDoesNotExist()
    {
        var repository = new InMemoryCompanyUserRepository();
        var accessService = new AccessService(repository);
        var companyPublicId = Guid.NewGuid();

        var canAccess = await accessService.CanAccessCompanyAsync(userId: 10, companyPublicId);

        Assert.False(canAccess);
    }

    [Fact]
    public async Task CanAccessCompanyAsync_ReturnsTrue_ForActiveCompanyUser()
    {
        var repository = new InMemoryCompanyUserRepository();
        var companyPublicId = Guid.NewGuid();
        await repository.AddAsync(CompanyUser.Create(companyPublicId, userId: 7, CompanyUserStatus.Active));
        var accessService = new AccessService(repository);

        var canAccess = await accessService.CanAccessCompanyAsync(7, companyPublicId);

        Assert.True(canAccess);
    }

    [Fact]
    public async Task CanAccessCompanyAsync_ReturnsFalse_ForInactiveCompanyUser()
    {
        var repository = new InMemoryCompanyUserRepository();
        var companyPublicId = Guid.NewGuid();
        await repository.AddAsync(CompanyUser.Create(companyPublicId, userId: 12, CompanyUserStatus.Disabled));
        var accessService = new AccessService(repository);

        var canAccess = await accessService.CanAccessCompanyAsync(12, companyPublicId);

        Assert.False(canAccess);
    }

    [Fact]
    public async Task CanAccessCompanyAsync_ReturnsFalse_ForSuspendedCompanyUser()
    {
        var repository = new InMemoryCompanyUserRepository();
        var companyPublicId = Guid.NewGuid();
        await repository.AddAsync(CompanyUser.Create(companyPublicId, userId: 21, CompanyUserStatus.Suspended));
        var accessService = new AccessService(repository);

        var canAccess = await accessService.CanAccessCompanyAsync(21, companyPublicId);

        Assert.False(canAccess);
    }

    [Fact]
    public async Task CanAccessCompanyAsync_ReturnsFalse_WhenInputsAreInvalid()
    {
        var repository = new InMemoryCompanyUserRepository();
        var accessService = new AccessService(repository);

        var missingCompany = await accessService.CanAccessCompanyAsync(1, Guid.Empty);
        var missingUser = await accessService.CanAccessCompanyAsync(0, Guid.NewGuid());

        Assert.False(missingCompany);
        Assert.False(missingUser);
    }

    private sealed class InMemoryCompanyUserRepository : ICompanyUserRepository
    {
        private readonly List<CompanyUser> _companyUsers = new();

        public Task<CompanyUser?> GetAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_companyUsers.SingleOrDefault(user => user.CompanyPublicId == companyPublicId && user.UserId == userId));

        public Task<bool> IsActiveMemberAsync(Guid companyPublicId, long userId, CancellationToken cancellationToken = default)
        {
            var member = _companyUsers.FirstOrDefault(user => user.CompanyPublicId == companyPublicId && user.UserId == userId && user.Status == CompanyUserStatus.Active);
            return Task.FromResult(member != null);
        }

        public Task<Guid?> GetActiveCompanyPublicIdAsync(long userId, CancellationToken cancellationToken = default)
        {
            var active = _companyUsers.FirstOrDefault(user => user.UserId == userId && user.Status == CompanyUserStatus.Active);
            return Task.FromResult(active?.CompanyPublicId);
        }

        public Task AddAsync(CompanyUser companyUser, CancellationToken cancellationToken = default)
        {
            _companyUsers.Add(companyUser);
            return Task.CompletedTask;
        }
    }
}

using ERP.Modules.Users.Application.Repositories;
using ERP.Modules.Users.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Users.Application.Services;

public sealed class UserProfileService
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileService(IUserProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfile>> CreateAsync(
        long userId,
        string email,
        string firstName,
        string lastName,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (await _profileRepository.ExistsByEmailAsync(email, cancellationToken: cancellationToken))
        {
            return Result<UserProfile>.Fail($"User with email '{email}' already exists.");
        }

        var profile = UserProfile.Create(userId, email, firstName, lastName, displayName);
        await _profileRepository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserProfile>.Ok(profile);
    }

    public Task<IReadOnlyList<UserProfile>> ListAsync(long userId, CancellationToken cancellationToken = default)
        => _profileRepository.ListAsync(userId, cancellationToken);

    public Task<UserProfile?> GetAsync(long id, CancellationToken cancellationToken = default)
        => _profileRepository.GetByIdAsync(id, cancellationToken);

    public Task<UserProfile?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        => _profileRepository.GetByUserIdAsync(userId, cancellationToken);

    public async Task<Result<UserProfile>> UpdateAsync(
        long userId,
        long id,
        string email,
        string firstName,
        string lastName,
        string? displayName,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByIdAsync(id, cancellationToken);
        if (profile is null || profile.UserId != userId)
        {
            return Result<UserProfile>.Fail("User profile not found.");
        }

        if (await _profileRepository.ExistsByEmailAsync(email, profile.Id, cancellationToken))
        {
            return Result<UserProfile>.Fail($"User with email '{email}' already exists.");
        }

        profile.Update(email, firstName, lastName, displayName, isActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserProfile>.Ok(profile);
    }

    public async Task<Result> DeleteAsync(long userId, long id, CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByIdAsync(id, cancellationToken);
        if (profile is null || profile.UserId != userId)
        {
            return Result.Fail("User profile not found.");
        }

        _profileRepository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

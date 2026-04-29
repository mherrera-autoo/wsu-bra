using ERP.Modules.Identity.Application.Services;
using ERP.Shared.Application;

namespace ERP.Documents.Application.Handlers;

internal sealed class DocumentAccessValidator
{
    private static readonly string[] ElevatedRoles =
    [
        "Admin",
        "SuperAdmin",
        "Documents.Admin",
        "DocumentsManager"
    ];

    private readonly IRbacService _rbacService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public DocumentAccessValidator(IRbacService rbacService, ICurrentUserProvider currentUserProvider)
    {
        _rbacService = rbacService;
        _currentUserProvider = currentUserProvider;
    }

    public Result ValidateUser(long userId)
    {
        if (userId <= 0)
        {
            return Result.Fail("User is required.");
        }

        if (_currentUserProvider.UserId.HasValue && _currentUserProvider.UserId.Value != userId)
        {
            return Result.Fail("User is not authorized to act on behalf of another user.");
        }

        return Result.Ok();
    }

    public async Task<Result> EnsurePermissionAsync(long userId, string permissionKey, CancellationToken cancellationToken)
    {
        if (HasElevatedRole())
        {
            return Result.Ok();
        }

        var hasPermission = await _rbacService.HasPermissionAsync(userId, permissionKey, cancellationToken);
        if (!hasPermission)
        {
            return Result.Fail("User does not have permission for this action.");
        }

        return Result.Ok();
    }

    private bool HasElevatedRole()
        => _currentUserProvider.Roles.Any(role => ElevatedRoles.Any(elevated =>
            string.Equals(role, elevated, StringComparison.OrdinalIgnoreCase)));
}

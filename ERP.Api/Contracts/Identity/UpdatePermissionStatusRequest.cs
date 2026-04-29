namespace ERP.Api.Contracts.Identity;

public sealed record UpdatePermissionStatusRequest
{
    public bool IsActive { get; init; }
}

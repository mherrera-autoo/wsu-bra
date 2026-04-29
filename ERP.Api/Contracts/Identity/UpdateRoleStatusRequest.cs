namespace ERP.Api.Contracts.Identity;

public sealed record UpdateRoleStatusRequest
{
    public bool IsActive { get; init; }
}

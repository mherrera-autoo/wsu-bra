namespace ERP.Modules.Identity.Application.Services;

public interface IPasswordReuseValidator
{
    Task EnsureNotReusedAsync(long userId, string newPasswordHash, CancellationToken cancellationToken = default);
}
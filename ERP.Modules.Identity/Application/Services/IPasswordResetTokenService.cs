namespace ERP.Modules.Identity.Application.Services;

public interface IPasswordResetTokenService
{
    string GenerateToken();
    string HashToken(string token);
}
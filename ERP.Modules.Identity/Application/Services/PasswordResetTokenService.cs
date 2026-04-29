using System.Security.Cryptography;
using ERP.Modules.Identity.Application.Services;

namespace ERP.Modules.Identity.Application.Services;

public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    private const int TokenSizeBytes = 32;

    public string GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string HashToken(string token)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
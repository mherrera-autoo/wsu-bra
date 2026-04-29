using System.Security.Cryptography;
using ERP.Modules.Identity.Application.Security;

namespace ERP.Api.Authentication;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int DefaultIterations = 100_000;

    public (string hash, string salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string passwordHash, string passwordSalt)
    {
        var salt = Convert.FromBase64String(passwordSalt);
        var expectedHash = Convert.FromBase64String(passwordHash);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

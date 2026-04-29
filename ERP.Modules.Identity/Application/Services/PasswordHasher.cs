using System.Security.Cryptography;

namespace ERP.Modules.Identity.Application.Services;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static (string hash, string salt) Hash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expectedHash = Convert.FromBase64String(hash);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}

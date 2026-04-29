using ERP.Modules.Identity.Application.Security;

namespace ERP.Modules.Identity.Application.Services;

public sealed class PasswordHasherWrapper : IPasswordHasher
{
    public (string hash, string salt) HashPassword(string password)
    {
        return PasswordHasher.Hash(password);
    }

    public bool VerifyPassword(string password, string passwordHash, string passwordSalt)
    {
        return PasswordHasher.Verify(password, passwordHash, passwordSalt);
    }
}
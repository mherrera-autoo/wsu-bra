namespace ERP.Modules.Identity.Application.Security;

public interface IPasswordHasher
{
    (string hash, string salt) HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash, string passwordSalt);
}

using ERP.Modules.Identity.Application.Commands;
using ERP.Modules.Identity.Application.Options;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Notifications.Contracts.Notifications;
using ERP.Shared.Application;
using Microsoft.Extensions.Options;

namespace ERP.Modules.Identity.Application.Services;

public sealed class PasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUserPasswordHistoryRepository _userPasswordHistoryRepository;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordReuseValidator _passwordReuseValidator;
    private readonly IEmailSender _emailSender;
    private readonly ISessionInvalidationService _sessionInvalidationService;
    private readonly IOptions<PasswordResetOptions> _passwordResetOptions;
    private readonly IOptions<PasswordReusePolicyOptions> _passwordReusePolicyOptions;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUserPasswordHistoryRepository userPasswordHistoryRepository,
        IPasswordResetTokenService passwordResetTokenService,
        IPasswordHasher passwordHasher,
        IPasswordReuseValidator passwordReuseValidator,
        IEmailSender emailSender,
        ISessionInvalidationService sessionInvalidationService,
        IOptions<PasswordResetOptions> passwordResetOptions,
        IOptions<PasswordReusePolicyOptions> passwordReusePolicyOptions)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userPasswordHistoryRepository = userPasswordHistoryRepository;
        _passwordResetTokenService = passwordResetTokenService;
        _passwordHasher = passwordHasher;
        _passwordReuseValidator = passwordReuseValidator;
        _emailSender = emailSender;
        _sessionInvalidationService = sessionInvalidationService;
        _passwordResetOptions = passwordResetOptions;
        _passwordReusePolicyOptions = passwordReusePolicyOptions;
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email.ToLowerInvariant().Trim(), cancellationToken);
        
        // Always return success to prevent email enumeration attacks
        if (user == null)
        {
            return Result.Ok();
        }

        var token = _passwordResetTokenService.GenerateToken();
        var tokenHash = _passwordResetTokenService.HashToken(token);
        var expiresAt = DateTime.UtcNow.AddMinutes(_passwordResetOptions.Value.TokenExpiryMinutes);
        
        var passwordResetToken = PasswordResetToken.Create(user.Id, tokenHash, expiresAt);
        await _passwordResetTokenRepository.CreateAsync(passwordResetToken, cancellationToken);

        var resetUrl = $"{_passwordResetOptions.Value.FrontendResetUrl}?token={token}";
        var emailMessage = new EmailMessage(
            user.Email,
            "Recuperación de Contraseña - AutooERP",
            $@"
                <h2>Recuperación de Contraseña</h2>
                <p>Ha solicitado restablecer su contraseña. Haga clic en el siguiente enlace:</p>
                <p><a href='{resetUrl}'>Restablecer Contraseña</a></p>
                <p>Este enlace expirará en 15 minutos.</p>
                <p>Si no solicitó esta acción, puede ignorar este correo.</p>
                <br>
                <p>El equipo de AutooERP</p>
            "
        );

        await _emailSender.SendAsync(emailMessage, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var tokenHash = _passwordResetTokenService.HashToken(command.Token);
        var resetToken = await _passwordResetTokenRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken);
        
        if (resetToken == null)
        {
            return Result.Fail("Token inválido o expirado.");
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Fail("Usuario no encontrado.");
        }

        // Validate password reuse policy
        await _passwordReuseValidator.EnsureNotReusedAsync(user.Id, command.NewPassword, cancellationToken);

        // Hash new password
        var (newPasswordHash, newPasswordSalt) = _passwordHasher.HashPassword(command.NewPassword);

        // Store current password in history
        var passwordHistory = UserPasswordHistory.Create(user.Id, user.PasswordHash, user.PasswordSalt);
        await _userPasswordHistoryRepository.AddAsync(passwordHistory, cancellationToken);

        // Prune old password history
        var disallowLastN = _passwordReusePolicyOptions.Value.DisallowLastN;
        await _userPasswordHistoryRepository.PruneAsync(user.Id, disallowLastN, cancellationToken);

        // Update user password
        user.UpdatePassword(newPasswordHash, newPasswordSalt);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate all user sessions (forcing re-login with new password)
        await _sessionInvalidationService.InvalidateUserSessionsAsync(user.Id, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(long userId, ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result.Fail("Usuario no encontrado.");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return Result.Fail("Contraseña actual incorrecta.");
        }

        // Validate password reuse policy
        await _passwordReuseValidator.EnsureNotReusedAsync(user.Id, command.NewPassword, cancellationToken);

        // Hash new password
        var (newPasswordHash, newPasswordSalt) = _passwordHasher.HashPassword(command.NewPassword);

        // Store current password in history
        var passwordHistory = UserPasswordHistory.Create(user.Id, user.PasswordHash, user.PasswordSalt);
        await _userPasswordHistoryRepository.AddAsync(passwordHistory, cancellationToken);

        // Prune old password history
        var disallowLastN = _passwordReusePolicyOptions.Value.DisallowLastN;
        await _userPasswordHistoryRepository.PruneAsync(user.Id, disallowLastN, cancellationToken);

        // Update user password
        user.UpdatePassword(newPasswordHash, newPasswordSalt);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate all user sessions (forcing re-login with new password)
        await _sessionInvalidationService.InvalidateUserSessionsAsync(user.Id, cancellationToken);

        return Result.Ok();
    }
}

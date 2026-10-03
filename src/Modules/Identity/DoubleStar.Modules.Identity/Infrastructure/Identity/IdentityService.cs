using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Application.Errors;
using DoubleStar.Modules.Identity.Domain.Enums;
using DoubleStar.Modules.Identity.Infrastructure.Tokens;
using DoubleStar.Modules.Identity.Persistence;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    ApplicationIdentityDbContext dbContext,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions,
    IOptions<FrontendOptions> frontendOptions)
    : IIdentityService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly FrontendOptions _frontendOptions = frontendOptions.Value;

    public async Task<Result<Guid>> RegisterCustomerAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Guid>.Failure(AuthErrors.EmailRequired());
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result<Guid>.Failure(AuthErrors.EmailAlreadyExists());
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result<Guid>.Failure(
                AuthErrors.RegistrationFailed(Describe(result)));
        }

        await userManager.AddToRoleAsync(user, "Customer");

        var confirmToken =
            await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmLink =
            $"{_frontendOptions.CustomerAppUrl}/confirm-email" +
            $"?email={Uri.EscapeDataString(normalizedEmail)}" +
            $"&token={Uri.EscapeDataString(confirmToken)}";

        await emailSender.SendEmailConfirmationAsync(
            normalizedEmail,
            confirmLink,
            cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
    public async Task<Result<Guid>> RegisterStaffAsync(
        string firstName, string lastName, string email, string password, string role,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StaffRole>(role, ignoreCase: true, out var staffRole))
        {
            return Result<Guid>.Failure(AuthErrors.InvalidRole(role));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result<Guid>.Failure(AuthErrors.EmailAlreadyExists());
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            EmailConfirmed = true, // admin-created staff accounts skip confirmation
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result<Guid>.Failure(AuthErrors.RegistrationFailed(Describe(result)));
        }

        await userManager.AddToRoleAsync(user, staffRole.ToString());
        return Result<Guid>.Success(user.Id);
    }

    public async Task<LoginOutcome> LoginAsync(string emailOrPhone, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = emailOrPhone.Trim();

        var user = await userManager.FindByEmailAsync(normalized.ToLowerInvariant())
            ?? await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalized, cancellationToken);

        if (user is null)
        {
            return new LoginOutcome.InvalidCredentials();
        }

        if (!user.IsActive)
        {
            return new LoginOutcome.AccountInactive();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            return new LoginOutcome.AccountLockedOut(user.LockoutEnd);
        }
        if (!signInResult.Succeeded)
        {
            return new LoginOutcome.InvalidCredentials();
        }

        if (!user.EmailConfirmed)
        {
            return new LoginOutcome.EmailNotConfirmed();
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var challengeToken = await CreateTwoFactorChallengeAsync(user.Id, cancellationToken);
            return new LoginOutcome.TwoFactorRequired(challengeToken);
        }

        return new LoginOutcome.Success(await BuildAuthenticationResultAsync(user, cancellationToken));
    }

    public async Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHasher.Hash(refreshToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (stored is null || !stored.IsActive)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.ReplacedByTokenHash = RefreshTokenHasher.Hash(newRefreshToken);
        await StoreRefreshTokenAsync(user.Id, newRefreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildAuthenticationResultAsync(user, cancellationToken, newRefreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHasher.Hash(refreshToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (stored is null || !stored.IsActive)
        {
            return false;
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded ? Result.Success() : Result.Failure(AuthErrors.ChangePasswordFailed(Describe(result)));
    }

    public async Task<Result> DeactivateStaffAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var roles = await userManager.GetRolesAsync(user);
        return user.ToDto(roles.ToList());
    }

    private async Task<AuthenticationResult> BuildAuthenticationResultAsync(
        ApplicationUser user, CancellationToken cancellationToken, string? existingRefreshToken = null)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = existingRefreshToken ?? await tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        if (existingRefreshToken is null)
        {
            await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);
        }

        return new AuthenticationResult(
            user.Id, user.Email, user.PhoneNumber, $"{user.FirstName} {user.LastName}".Trim(),
            accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            roles.ToList());
    }

    private async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = RefreshTokenHasher.Hash(refreshToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Describe(IdentityResult result) => string.Join("; ", result.Errors.Select(e => e.Description));

    public async Task<IReadOnlyList<UserProfileDto>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var users = await userManager.Users.OrderBy(u => u.Email).ToListAsync(cancellationToken);
        var result = new List<UserProfileDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(user.ToDto(roles.ToList()));
        }

        return result;
    }

    public async Task<Result> UpdateStaffAsync(
        Guid userId, string firstName, string lastName, string role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.TryParse<StaffRole>(role, ignoreCase: true, out var staffRole))
        {
            return Result.Failure(AuthErrors.InvalidRole(role));
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure(AuthErrors.RegistrationFailed(Describe(updateResult)));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Where(r => r != staffRole.ToString()).ToList();
        if (rolesToRemove.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (!currentRoles.Contains(staffRole.ToString()))
        {
            await userManager.AddToRoleAsync(user, staffRole.ToString());
        }

        return Result.Success();
    }

    public async Task<Result> ReactivateStaffAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.IsActive = true;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> UpdateOwnProfileAsync(
        Guid userId, string firstName, string lastName, string? phone, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.PhoneNumber = phone?.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? Result.Success() : Result.Failure(AuthErrors.RegistrationFailed(Describe(result)));
    }

    public async Task<Result> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.AvatarUrl = avatarUrl;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> RemoveAvatarAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        user.AvatarUrl = null;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<TwoFactorSetupDto>> BeginTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<TwoFactorSetupDto>.Failure(UserErrors.NotFound(userId));

        await userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);

        var issuer = Uri.EscapeDataString("Double Star");
        var label = Uri.EscapeDataString(user.Email ?? user.UserName ?? userId.ToString());
        var authenticatorUri = $"otpauth://totp/{issuer}:{label}?secret={unformattedKey}&issuer={issuer}&digits=6";

        return Result<TwoFactorSetupDto>.Success(new TwoFactorSetupDto(unformattedKey!, authenticatorUri));
    }

    public async Task<Result> ConfirmTwoFactorSetupAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        if (!await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code))
        {
            return Result.Failure(new Error("Auth.InvalidTwoFactorCode", "That code is incorrect or has expired.", ErrorType.Validation));
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        return Result.Success();
    }

    public async Task<Result> DisableTwoFactorAsync(Guid userId, string currentPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        if (!await userManager.CheckPasswordAsync(user, currentPassword))
        {
            return Result.Failure(new Error("Auth.InvalidCredentials", "The current password is incorrect.", ErrorType.Unauthorized));
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        return Result.Success();
    }

    public async Task<Result> RequestEmailSignInCodeAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null || !user.IsActive)
        {
            return Result.Success(); // never reveal whether the account exists
        }

        var code = GenerateNumericCode();
        var linkToken = GenerateOpaqueToken();

        await dbContext.EmailSignInCodes.AddAsync(new EmailSignInCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = OpaqueTokenHasher.Hash(code),
            LinkTokenHash = OpaqueTokenHasher.Hash(linkToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var magicLink = $"{_frontendOptions.CustomerAppUrl}/email-signin?token={Uri.EscapeDataString(linkToken)}";
        await emailSender.SendSignInCodeAsync(user.Email!, code, magicLink, cancellationToken);
        return Result.Success();
    }

    public async Task<LoginOutcome> SignInWithEmailCodeAsync(string email, string code, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || !user.IsActive)
        {
            return new LoginOutcome.InvalidCredentials();
        }

        var codeHash = OpaqueTokenHasher.Hash(code);
        var storedCode = await dbContext.EmailSignInCodes
            .Where(c => c.UserId == user.Id && c.CodeHash == codeHash)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (storedCode is null || !storedCode.IsActive)
        {
            return new LoginOutcome.InvalidTwoFactorCode();
        }

        storedCode.ConsumedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginOutcome.Success(await BuildAuthenticationResultAsync(user, cancellationToken));
    }

    public async Task<LoginOutcome> SignInWithEmailLinkAsync(string linkToken, CancellationToken cancellationToken)
    {
        var tokenHash = OpaqueTokenHasher.Hash(linkToken);
        var storedCode = await dbContext.EmailSignInCodes.FirstOrDefaultAsync(c => c.LinkTokenHash == tokenHash, cancellationToken);
        if (storedCode is null || !storedCode.IsActive)
        {
            return new LoginOutcome.InvalidTwoFactorCode();
        }

        var user = await userManager.FindByIdAsync(storedCode.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return new LoginOutcome.InvalidCredentials();
        }

        storedCode.ConsumedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginOutcome.Success(await BuildAuthenticationResultAsync(user, cancellationToken));
    }

    public async Task<LoginOutcome> VerifyTwoFactorChallengeAsync(string challengeToken, string code, CancellationToken cancellationToken)
    {
        var tokenHash = OpaqueTokenHasher.Hash(challengeToken);
        var challenge = await dbContext.TwoFactorChallenges.FirstOrDefaultAsync(c => c.TokenHash == tokenHash, cancellationToken);
        if (challenge is null || !challenge.IsActive)
        {
            return new LoginOutcome.InvalidTwoFactorCode();
        }

        var user = await userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return new LoginOutcome.InvalidCredentials();
        }

        if (!await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code))
        {
            return new LoginOutcome.InvalidTwoFactorCode();
        }

        challenge.ConsumedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginOutcome.Success(await BuildAuthenticationResultAsync(user, cancellationToken));
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive) return null;
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return Result.Failure(AuthErrors.InvalidCredentials());

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded ? Result.Success() : Result.Failure(AuthErrors.ChangePasswordFailed(Describe(result)));
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return null;
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return Result.Failure(AuthErrors.InvalidCredentials());

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Result.Success() : Result.Failure(AuthErrors.RegistrationFailed(Describe(result)));
    }

    private async Task<string> CreateTwoFactorChallengeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rawToken = GenerateOpaqueToken();

        await dbContext.TwoFactorChallenges.AddAsync(new TwoFactorChallenge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = OpaqueTokenHasher.Hash(rawToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    private static string GenerateNumericCode()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        var value = BitConverter.ToUInt32(bytes) % 1_000_000;
        return value.ToString("D6");
    }

    private static string GenerateOpaqueToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
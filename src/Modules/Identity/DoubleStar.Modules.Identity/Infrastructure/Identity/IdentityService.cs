// Infrastructure/Identity/IdentityService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
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
    IOptions<JwtOptions> jwtOptions)
    : IIdentityService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<Guid>> RegisterCustomerAsync(
        string firstName, string lastName, string? email, string? phone, string password,
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
            PhoneNumber = phone?.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            EmailConfirmed = true, // no email-confirmation flow in v1
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result<Guid>.Failure(AuthErrors.RegistrationFailed(Describe(result)));
        }

        await userManager.AddToRoleAsync(user, "Customer");

        // Hook point: once the Customers module exists, publish a
        // CustomerAccountRegisteredEvent here so it can create its own
        // Customer record (name/phone/email) linked by this UserId.

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
            EmailConfirmed = true,
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
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(AuthErrors.ChangePasswordFailed(Describe(result)));
    }

    public async Task<Result> DeactivateStaffAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }
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
}
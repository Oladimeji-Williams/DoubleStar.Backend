// Abstractions/Authentication/IIdentityService.cs
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public interface IIdentityService
{
    /// <summary>Public self-service signup — always assigned the Customer role.</summary>
    Task<Result<Guid>> RegisterCustomerAsync(
            string email, string password,
            CancellationToken cancellationToken);
    /// <summary>Admin-only — creates a staff account with a specific role.</summary>
    Task<Result<Guid>> RegisterStaffAsync(
        string firstName, string lastName, string email, string password, string role,
        CancellationToken cancellationToken);
    Task<LoginOutcome> LoginAsync(string emailOrPhone, string password, CancellationToken cancellationToken);
    Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<Result> DeactivateStaffAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserProfileDto>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<Result> UpdateStaffAsync(Guid userId, string firstName, string lastName, string role, CancellationToken cancellationToken);
    Task<Result> ReactivateStaffAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> UpdateOwnProfileAsync(Guid userId, string firstName, string lastName, string? phone, CancellationToken cancellationToken);
    Task<Result> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken);
    Task<Result> RemoveAvatarAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<TwoFactorSetupDto>> BeginTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> ConfirmTwoFactorSetupAsync(Guid userId, string code, CancellationToken cancellationToken);
    Task<Result> DisableTwoFactorAsync(Guid userId, string currentPassword, CancellationToken cancellationToken);
    Task<Result> RequestEmailSignInCodeAsync(string email, CancellationToken cancellationToken);
    Task<LoginOutcome> SignInWithEmailCodeAsync(string email, string code, CancellationToken cancellationToken);
    Task<LoginOutcome> SignInWithEmailLinkAsync(string linkToken, CancellationToken cancellationToken);
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    Task<string?> GenerateEmailConfirmationTokenAsync(string email, CancellationToken cancellationToken);
    Task<Result> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken);
    Task<LoginOutcome> VerifyTwoFactorChallengeAsync(string challengeToken, string code, CancellationToken cancellationToken);
}
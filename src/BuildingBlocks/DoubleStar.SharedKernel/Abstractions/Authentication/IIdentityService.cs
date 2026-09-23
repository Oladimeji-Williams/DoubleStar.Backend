// Abstractions/Authentication/IIdentityService.cs
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public interface IIdentityService
{
    /// <summary>Public self-service signup — always assigned the Customer role.</summary>
    Task<Result<Guid>> RegisterCustomerAsync(
        string firstName, string lastName, string? email, string? phone, string password,
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
}
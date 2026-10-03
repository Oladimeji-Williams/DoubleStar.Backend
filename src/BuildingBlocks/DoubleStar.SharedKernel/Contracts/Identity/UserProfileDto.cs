// SharedKernel/Contracts/Identity/UserProfileDto.cs — full replacement
namespace DoubleStar.SharedKernel.Contracts.Identity;

public sealed record UserProfileDto(
    Guid Id,
    string? Email,
    string? Phone,
    string? FirstName,
    string? LastName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    string? AvatarUrl);
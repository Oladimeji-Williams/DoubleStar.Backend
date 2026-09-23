// Infrastructure/Identity/UserMappings.cs
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

internal static class UserMappings
{
    public static UserProfileDto ToDto(this ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email, user.PhoneNumber, user.FirstName, user.LastName, roles);
}
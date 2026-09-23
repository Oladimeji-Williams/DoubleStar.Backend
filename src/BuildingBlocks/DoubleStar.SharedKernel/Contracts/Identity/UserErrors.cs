// Contracts/Identity/UserErrors.cs
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Identity;

public static class UserErrors
{
    public static Error NotFound(Guid id) => new(
        "Identity.NotFound", $"User with ID '{id}' was not found.", ErrorType.NotFound);

    public static Error NotAuthenticated() => new(
        "Identity.NotAuthenticated", "You must be signed in to perform this action.", ErrorType.Unauthorized);
}
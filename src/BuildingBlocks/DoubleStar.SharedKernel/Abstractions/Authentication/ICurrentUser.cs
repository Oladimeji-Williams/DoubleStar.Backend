// Abstractions/Authentication/ICurrentUser.cs
namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);

    /// <summary>True if the signed-in account is a customer, not staff.</summary>
    bool IsCustomer { get; }

    /// <summary>True if the signed-in account is any staff role.</summary>
    bool IsStaff { get; }
}
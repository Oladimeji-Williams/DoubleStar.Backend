using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Customers.Domain.Entities;

public sealed class Customer : GuidEntity
{
    public string? Name { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public bool HasAccount { get; private set; }

    private Customer()
    {
    }

    /// <summary>
    /// Creates a walk-in customer with no login account.
    /// A walk-in customer must have a name.
    /// </summary>
    public static Customer CreateWalkIn(
        string name,
        string? phone,
        string? email) => new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            HasAccount = false,
        };

    /// <summary>
    /// Creates a customer linked to an Identity ApplicationUser.
    /// The Customer Id deliberately matches the Identity User Id.
    ///
    /// Self-service registration only requires email and password,
    /// so name and phone are completed later through the profile.
    /// </summary>
    public static Customer CreateForAccount(
        Guid userId,
        string? email) => new()
        {
            Id = userId,
            Email = email?.Trim(),
            HasAccount = true,
        };

    /// <summary>
    /// Updates the customer's profile details.
    /// Name is required when completing/updating the profile.
    /// </summary>
    public void UpdateDetails(
        string name,
        string? phone,
        string? email,
        string? address)
    {
        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
    }
}
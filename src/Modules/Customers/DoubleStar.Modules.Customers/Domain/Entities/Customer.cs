// Domain/Entities/Customer.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Customers.Domain.Entities;

public sealed class Customer : GuidEntity
{
    public string Name { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public bool HasAccount { get; private set; }

    private Customer() { }

    /// <summary>Walk-in customer with no login — Id is a fresh Guid.</summary>
    public static Customer CreateWalkIn(string name, string? phone, string? email) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Phone = phone?.Trim(),
        Email = email?.Trim(),
        HasAccount = false,
    };

    /// <summary>
    /// Id is deliberately the same Guid as Identity's ApplicationUser.Id —
    /// that's how the two records line up without a cross-schema FK.
    /// </summary>
    public static Customer CreateForAccount(Guid userId, string name, string? phone, string? email) => new()
    {
        Id = userId,
        Name = name.Trim(),
        Phone = phone?.Trim(),
        Email = email?.Trim(),
        HasAccount = true,
    };

    public void UpdateDetails(string name, string? phone, string? email, string? address)
    {
        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
    }
}
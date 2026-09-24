// SeedData/StaffSeedData.cs
using Microsoft.AspNetCore.Identity;
using DoubleStar.Modules.Identity.Domain.Enums;
using DoubleStar.Modules.Identity.Infrastructure.Identity;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class StaffSeedData
{
    public static async Task<(ApplicationUser Manager, ApplicationUser Cashier, ApplicationUser Technician)> SeedAsync(
        UserManager<ApplicationUser> userManager)
    {
        var manager = await CreateStaffAsync(userManager, "manager@doublestar.local", "Amaka", "Eze", StaffRole.Manager);
        var cashier = await CreateStaffAsync(userManager, "cashier@doublestar.local", "Chidi", "Obi", StaffRole.Cashier);
        var technician = await CreateStaffAsync(userManager, "technician@doublestar.local", "Bola", "Adeyemi", StaffRole.Technician);
        return (manager, cashier, technician);
    }

    private static async Task<ApplicationUser> CreateStaffAsync(
        UserManager<ApplicationUser> userManager, string email, string firstName, string lastName, StaffRole role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email, Email = email, FirstName = firstName, LastName = lastName,
            EmailConfirmed = true, CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, "StaffPassword123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed staff '{email}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role.ToString());
        return user;
    }
}
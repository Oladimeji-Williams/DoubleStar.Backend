// Infrastructure/Identity/IdentitySeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Modules.Identity.Domain.Enums;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string BootstrapAdminEmail = "admin@doublestar.local";
    private const string BootstrapAdminPassword = "ChangeMe123!"; // change immediately after first login

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Enum.GetNames<StaffRole>().Append("Customer"))
        {
            await SeedRoleAsync(roleManager, role);
        }

        if (await userManager.FindByEmailAsync(BootstrapAdminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = BootstrapAdminEmail,
                Email = BootstrapAdminEmail,
                FirstName = "Store",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(admin, BootstrapAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, nameof(StaffRole.Admin));
            }
        }
    }

    private static async Task SeedRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(
            new ApplicationRole { Id = Guid.NewGuid(), Name = roleName, NormalizedName = roleName.ToUpperInvariant() });

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
        }
    }
}
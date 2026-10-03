using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Modules.Identity.Domain.Enums;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["Admin:Email"]
            ?? "admin@doublestar.local";

        var adminPassword = configuration["Admin:Password"]
            ?? "ChangeMe123!";

        var resetAdminPassword = configuration.GetValue<bool>(
            "Admin:ResetPassword");

        // Seed roles
        foreach (var role in Enum.GetNames<StaffRole>().Append("Customer"))
        {
            await SeedRoleAsync(roleManager, role);
        }

        // Seed admin
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Store",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow,
            };

            var createResult = await userManager.CreateAsync(
                admin,
                adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to seed admin user '{adminEmail}': {errors}");
            }
        }
        else if (resetAdminPassword)
        {
            await ResetPasswordAsync(
                userManager,
                admin,
                adminPassword);
        }

        // Make sure the admin has the Admin role.
        if (!await userManager.IsInRoleAsync(
                admin,
                nameof(StaffRole.Admin)))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin,
                nameof(StaffRole.Admin));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to assign Admin role to '{adminEmail}': {errors}");
            }
        }
    }

    private static async Task ResetPasswordAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser admin,
        string newPassword)
    {
        var removeResult = await userManager.RemovePasswordAsync(admin);

        if (!removeResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                removeResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to remove existing admin password: {errors}");
        }

        var addResult = await userManager.AddPasswordAsync(
            admin,
            newPassword);

        if (!addResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                addResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to set new admin password: {errors}");
        }
    }

    private static async Task SeedRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(
            new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to seed role '{roleName}': {errors}");
        }
    }
}
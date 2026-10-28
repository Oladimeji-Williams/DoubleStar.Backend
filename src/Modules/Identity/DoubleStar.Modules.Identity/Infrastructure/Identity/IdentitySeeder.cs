
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Modules.Identity.Domain.Enums;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var configuration =
            services.GetRequiredService<IConfiguration>();

        var roleManager =
            services.GetRequiredService<RoleManager<ApplicationRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail =
            configuration["Admin:Email"]
            ?? "admin@doublestar.local";

        var adminPassword =
            configuration["Admin:Password"]
            ?? "ChangeMe123!";

        var resetAdminPassword =
            configuration.GetValue<bool>("Admin:ResetPassword");

        // Seed all staff roles and the Customer role.
        foreach (var role in Enum.GetNames<StaffRole>().Append("Customer"))
        {
            await SeedRoleAsync(roleManager, role);
        }

        // Find the existing admin account.
        var admin =
            await userManager.FindByEmailAsync(adminEmail);

        // Create the admin account if it does not exist.
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
                CreatedAtUtc = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(admin, adminPassword);

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
            // Reset the password only when explicitly configured.
            await ResetPasswordAsync(
                userManager,
                admin,
                adminPassword);
        }

        // Ensure the account has the Admin role.
        var isAdmin =
            await userManager.IsInRoleAsync(
                admin,
                nameof(StaffRole.Admin));

        if (!isAdmin)
        {
            var roleResult =
                await userManager.AddToRoleAsync(
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

        // Ensure the admin account is active.
        if (!admin.IsActive)
        {
            admin.IsActive = true;

            var updateResult =
                await userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to activate admin account '{adminEmail}': {errors}");
            }
        }

        // Ensure the admin email is confirmed.
        if (!admin.EmailConfirmed)
        {
            admin.EmailConfirmed = true;

            var updateResult =
                await userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to confirm admin email '{adminEmail}': {errors}");
            }
        }

        // Admin accounts must never be locked out by failed passwords.
        // Clear an active lockout before disabling lockout.
        var lockoutEnabled =
            await userManager.GetLockoutEnabledAsync(admin);

        if (lockoutEnabled)
        {
            if (await userManager.IsLockedOutAsync(admin))
            {
                var clearLockoutResult =
                    await userManager.SetLockoutEndDateAsync(
                        admin,
                        null);

                if (!clearLockoutResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        clearLockoutResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to clear the admin lockout for '{adminEmail}': {errors}");
                }
            }

            var resetFailuresResult =
                await userManager.ResetAccessFailedCountAsync(admin);

            if (!resetFailuresResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    resetFailuresResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to reset the admin failed-login count for '{adminEmail}': {errors}");
            }

            var disableLockoutResult =
                await userManager.SetLockoutEnabledAsync(
                    admin,
                    false);

            if (!disableLockoutResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    disableLockoutResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to disable admin lockout for '{adminEmail}': {errors}");
            }
        }
        else
        {
            // Lockout is already disabled. Do not call
            // SetLockoutEndDateAsync in this branch.
            var resetFailuresResult =
                await userManager.ResetAccessFailedCountAsync(admin);

            if (!resetFailuresResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    resetFailuresResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to reset the admin failed-login count for '{adminEmail}': {errors}");
            }
        }
    }

    private static async Task ResetPasswordAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser admin,
        string newPassword)
    {
        var removeResult =
            await userManager.RemovePasswordAsync(admin);

        if (!removeResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                removeResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to remove existing admin password: {errors}");
        }

        var addResult =
            await userManager.AddPasswordAsync(
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
        {
            return;
        }

        var result =
            await roleManager.CreateAsync(
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
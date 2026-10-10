using Microsoft.AspNetCore.Identity;
using TestApplication.Models;

namespace TestApplication.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { IdentityRoles.SuperAdmin, IdentityRoles.TenantAdmin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result, $"creating the {role} role");
            }
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configure BootstrapAdmin:Email and BootstrapAdmin:Password using environment variables before starting the application.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && user.TenantId.HasValue)
        {
            throw new InvalidOperationException(
                "The configured bootstrap super-admin email belongs to a tenant account. Configure a different email.");
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, "creating the bootstrap super-admin account");
        }

        if (!await userManager.IsInRoleAsync(user, IdentityRoles.SuperAdmin))
        {
            var roleResult = await userManager.AddToRoleAsync(user, IdentityRoles.SuperAdmin);
            EnsureSucceeded(roleResult, "assigning the super-admin role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed while {operation}: {errors}");
        }
    }
}

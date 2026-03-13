using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Data;

public static class DbInitializer
{
    public const string RoleAdmin = "Admin";
    public const string RoleInspector = "Inspector";
    public const string RoleViewer = "Viewer";

    private const string AdminEmail = "admin@local";
    private const string InspectorEmail = "inspector@local";
    private const string ViewerEmail = "viewer@local";
    private const string SeedPassword = "Test@123"; // Assignment only

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        string[] roleNames = { RoleAdmin, RoleInspector, RoleViewer };
        foreach (var name in roleNames)
        {
            if (await roleManager.RoleExistsAsync(name)) continue;
            await roleManager.CreateAsync(new IdentityRole(name));
        }

        await EnsureUserAsync(userManager, AdminEmail, RoleAdmin);
        await EnsureUserAsync(userManager, InspectorEmail, RoleInspector);
        await EnsureUserAsync(userManager, ViewerEmail, RoleViewer);
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) return;

        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, SeedPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}

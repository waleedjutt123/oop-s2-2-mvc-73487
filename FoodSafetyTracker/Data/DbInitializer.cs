using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Data;

public static class DbInitializer
{
    const string SeedPassword = "Test@123";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Inspector"));
            await roleManager.CreateAsync(new IdentityRole("Viewer"));
        }

        await EnsureUserAsync(userManager, "admin@local", "Admin");
        await EnsureUserAsync(userManager, "inspector@local", "Inspector");
        await EnsureUserAsync(userManager, "viewer@local", "Viewer");

        if (await context.Premises.AnyAsync()) return;
        await SeedBusinessDataAsync(context);
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string role)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, SeedPassword);
        await userManager.AddToRoleAsync(user, role);
    }

    private static async Task SeedBusinessDataAsync(ApplicationDbContext context)
    {
        var towns = new[] { "TownA", "TownB", "TownC" };
        var ratings = new[] { RiskRating.Low, RiskRating.Medium, RiskRating.High };

        var premisesList = new List<Premises>();
        for (var i = 1; i <= 12; i++)
        {
            var town = towns[(i - 1) % 3];
            premisesList.Add(new Premises
            {
                Name = $"Premises {i} - {town}",
                Address = $"{100 + i} Main Street",
                Town = town,
                RiskRating = ratings[(i - 1) % 3]
            });
        }
        context.Premises.AddRange(premisesList);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var rnd = new Random(42);
        var inspectionsList = new List<Inspection>();

        for (var i = 0; i < 25; i++)
        {
            var premises = premisesList[rnd.Next(premisesList.Count)];
            var monthsAgo = rnd.Next(0, 4);
            var inspectionDate = thisMonth.AddMonths(-monthsAgo).AddDays(rnd.Next(0, 28));
            var score = rnd.Next(0, 101);
            var outcome = score >= 60 ? InspectionOutcome.Pass : InspectionOutcome.Fail;
            inspectionsList.Add(new Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = inspectionDate,
                Score = score,
                Outcome = outcome,
                Notes = i % 3 == 0 ? "Routine inspection." : null
            });
        }
        context.Inspections.AddRange(inspectionsList);
        await context.SaveChangesAsync();

        var today = DateTime.Today;
        var followUpsList = new List<FollowUp>();
        for (var i = 0; i < 10; i++)
        {
            var inspection = inspectionsList[i % inspectionsList.Count];
            var isOverdueOpen = i < 4;
            var isClosed = i >= 6;
            var dueDate = isOverdueOpen
                ? today.AddDays(-(i + 1))
                : isClosed ? inspection.InspectionDate.AddDays(14) : today.AddDays(7);
            var status = isClosed ? FollowUpStatus.Closed : FollowUpStatus.Open;
            var closedDate = isClosed ? inspection.InspectionDate.AddDays(21) : (DateTime?)null;

            followUpsList.Add(new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = dueDate,
                Status = status,
                ClosedDate = closedDate
            });
        }
        context.FollowUps.AddRange(followUpsList);
        await context.SaveChangesAsync();
    }
}

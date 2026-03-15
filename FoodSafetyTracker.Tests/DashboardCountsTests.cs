using FoodSafetyTracker.Controllers;
using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafetyTracker.Tests;

public class DashboardCountsTests
{
    [Fact]
    public async Task DashboardCounts_WithSeededData_MatchExpected()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DashboardTest_" + Guid.NewGuid())
            .Options;

        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        await using (var seedContext = new ApplicationDbContext(options))
        {
            var p = new Premises { Name = "P1", Address = "A1", Town = "TownA", RiskRating = RiskRating.Low };
            seedContext.Premises.Add(p);
            await seedContext.SaveChangesAsync();

            var insp1 = new Inspection { PremisesId = p.Id, InspectionDate = thisMonth.AddDays(1), Score = 70, Outcome = InspectionOutcome.Pass };
            var insp2 = new Inspection { PremisesId = p.Id, InspectionDate = thisMonth.AddDays(5), Score = 40, Outcome = InspectionOutcome.Fail };
            var insp3 = new Inspection { PremisesId = p.Id, InspectionDate = thisMonth.AddMonths(-1), Score = 80, Outcome = InspectionOutcome.Pass };
            seedContext.Inspections.AddRange(insp1, insp2, insp3);
            await seedContext.SaveChangesAsync();

            seedContext.FollowUps.AddRange(
                new FollowUp { InspectionId = insp1.Id, DueDate = today.AddDays(-3), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = insp2.Id, DueDate = today.AddDays(10), Status = FollowUpStatus.Open }
            );
            await seedContext.SaveChangesAsync();
        }

        await using var context = new ApplicationDbContext(options);
        var controller = new DashboardController(context);
        var result = await controller.Index(null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<FoodSafetyTracker.Models.DashboardViewModel>(viewResult.Model);

        Assert.Equal(2, model.InspectionsThisMonth);
        Assert.Equal(1, model.FailedInspectionsThisMonth);
        Assert.Equal(1, model.OverdueOpenFollowUps);
    }
}

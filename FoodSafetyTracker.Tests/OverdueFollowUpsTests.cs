using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafetyTracker.Tests;

public class OverdueFollowUpsTests
{
    [Fact]
    public async Task OverdueOpenFollowUps_Query_ReturnsCorrectCount()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "OverdueTest_" + Guid.NewGuid())
            .Options;

        await using (var context = new ApplicationDbContext(options))
        {
            var premises = new Premises { Name = "P1", Address = "A1", Town = "TownA", RiskRating = RiskRating.Low };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var insp = new Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = DateTime.Today.AddDays(-10),
                Score = 50,
                Outcome = InspectionOutcome.Fail
            };
            context.Inspections.Add(insp);
            await context.SaveChangesAsync();

            var today = DateTime.Today;
            context.FollowUps.AddRange(
                new FollowUp { InspectionId = insp.Id, DueDate = today.AddDays(-5), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = insp.Id, DueDate = today.AddDays(-1), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = insp.Id, DueDate = today.AddDays(5), Status = FollowUpStatus.Open }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            var today = DateTime.Today;
            var premiseIds = await context.Premises.Select(p => p.Id).ToListAsync();
            var inspectionIds = await context.Inspections.Where(i => premiseIds.Contains(i.PremisesId)).Select(i => i.Id).ToListAsync();
            var overdueOpenCount = await context.FollowUps
                .CountAsync(f => f.DueDate < today && f.Status == FollowUpStatus.Open && inspectionIds.Contains(f.InspectionId));

            Assert.Equal(2, overdueOpenCount);
        }
    }
}

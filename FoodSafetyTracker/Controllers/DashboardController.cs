using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(string? town, RiskRating? riskRating, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var year = today.Year;
        var month = today.Month;

        var premisesQuery = _context.Premises.AsQueryable();
        if (!string.IsNullOrWhiteSpace(town))
            premisesQuery = premisesQuery.Where(p => p.Town == town);
        if (riskRating.HasValue)
            premisesQuery = premisesQuery.Where(p => p.RiskRating == riskRating.Value);

        var premiseIds = await premisesQuery.Select(p => p.Id).ToListAsync(ct);

        var inspectionsQuery = _context.Inspections.Where(i => premiseIds.Contains(i.PremisesId));

        var inspectionsThisMonth = await inspectionsQuery
            .CountAsync(i => i.InspectionDate.Year == year && i.InspectionDate.Month == month, ct);

        var failedThisMonth = await inspectionsQuery
            .CountAsync(i => i.InspectionDate.Year == year && i.InspectionDate.Month == month && i.Outcome == InspectionOutcome.Fail, ct);

        var inspectionIds = await inspectionsQuery.Select(i => i.Id).ToListAsync(ct);
        var overdueOpenFollowUps = await _context.FollowUps
            .CountAsync(f => f.DueDate < today && f.Status == FollowUpStatus.Open && inspectionIds.Contains(f.InspectionId), ct);

        var towns = await _context.Premises.Select(p => p.Town).Distinct().OrderBy(t => t).ToListAsync(ct);

        var model = new DashboardViewModel
        {
            InspectionsThisMonth = inspectionsThisMonth,
            FailedInspectionsThisMonth = failedThisMonth,
            OverdueOpenFollowUps = overdueOpenFollowUps,
            SelectedTown = town,
            SelectedRiskRating = riskRating,
            Towns = towns,
            RiskRatings = new List<RiskRating> { RiskRating.Low, RiskRating.Medium, RiskRating.High }
        };

        return View(model);
    }
}

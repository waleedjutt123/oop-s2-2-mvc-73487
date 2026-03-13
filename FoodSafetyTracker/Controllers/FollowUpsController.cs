using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector + "," + DbInitializer.RoleViewer)]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _context.FollowUps
            .Include(f => f.Inspection!)
            .ThenInclude(i => i!.Premises)
            .OrderByDescending(f => f.DueDate)
            .ToListAsync(cancellationToken);
        return View(list);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var followUp = await _context.FollowUps
            .Include(f => f.Inspection!)
            .ThenInclude(i => i!.Premises)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (followUp == null) return NotFound();
        return View(followUp);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateInspectionDropdownAsync(null, cancellationToken);
        return View(new FollowUp { DueDate = DateTime.Today, Status = FollowUpStatus.Open });
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp followUp, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections.FindAsync([followUp.InspectionId], cancellationToken);
        if (inspection != null && followUp.DueDate.Date < inspection.InspectionDate.Date)
        {
            _logger.LogWarning("FollowUp create rejected: DueDate {DueDate} is before InspectionDate {InspectionDate}. InspectionId: {InspectionId}",
                followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);
            ModelState.AddModelError(nameof(FollowUp.DueDate), "Due date cannot be before the inspection date.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(followUp);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("FollowUp created. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}, Status: {Status}",
                followUp.Id, followUp.InspectionId, followUp.Status);
            return RedirectToAction(nameof(Index));
        }
        await PopulateInspectionDropdownAsync(followUp.InspectionId, cancellationToken);
        return View(followUp);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var followUp = await _context.FollowUps.Include(f => f.Inspection).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (followUp == null) return NotFound();
        await PopulateInspectionDropdownAsync(followUp.InspectionId, cancellationToken);
        return View(followUp);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FollowUp followUp, CancellationToken cancellationToken)
    {
        if (id != followUp.Id) return NotFound();

        if (followUp.Status == FollowUpStatus.Closed && !followUp.ClosedDate.HasValue)
        {
            ModelState.AddModelError(nameof(FollowUp.ClosedDate), "Closed date is required when status is Closed.");
        }

        var inspection = await _context.Inspections.FindAsync([followUp.InspectionId], cancellationToken);
        if (inspection != null && followUp.DueDate.Date < inspection.InspectionDate.Date)
        {
            _logger.LogWarning("FollowUp edit rejected: DueDate {DueDate} is before InspectionDate {InspectionDate}. FollowUpId: {FollowUpId}",
                followUp.DueDate, inspection.InspectionDate, followUp.Id);
            ModelState.AddModelError(nameof(FollowUp.DueDate), "Due date cannot be before the inspection date.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate.HasValue)
                    _logger.LogInformation("FollowUp closed. FollowUpId: {FollowUpId}, ClosedDate: {ClosedDate}", followUp.Id, followUp.ClosedDate);
                _context.Update(followUp);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("FollowUp updated. FollowUpId: {FollowUpId}, Status: {Status}", followUp.Id, followUp.Status);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FollowUps.AnyAsync(e => e.Id == followUp.Id, cancellationToken)) return NotFound();
                throw;
            }
        }
        await PopulateInspectionDropdownAsync(followUp.InspectionId, cancellationToken);
        return View(followUp);
    }

    private async Task PopulateInspectionDropdownAsync(int? selectedId, CancellationToken cancellationToken)
    {
        var items = await _context.Inspections.Include(i => i.Premises).OrderByDescending(i => i.InspectionDate)
            .Select(i => new { i.Id, Display = $"#{i.Id} - " + (i.Premises != null ? i.Premises.Name : "") + " - " + i.InspectionDate.ToString("d") }).ToListAsync(cancellationToken);
        ViewData["InspectionId"] = new SelectList(items, "Id", "Display", selectedId);
    }
}

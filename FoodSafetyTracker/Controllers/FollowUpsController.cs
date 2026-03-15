using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using FoodSafetyTracker.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _context.FollowUps
            .Include(f => f.Inspection!)
            .ThenInclude(i => i!.Premises)
            .OrderByDescending(f => f.DueDate)
            .ToListAsync(ct);
        return View(list);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var f = await _context.FollowUps.Include(x => x.Inspection!).ThenInclude(i => i!.Premises)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
        if (f == null) return NotFound();
        return View(f);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateInspections(ct, null);
        return View(new FollowUp { DueDate = DateTime.Today, Status = FollowUpStatus.Open });
    }

    [Authorize(Roles = "Admin,Inspector")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp followUp, CancellationToken ct)
    {
        var inspection = await _context.Inspections.FindAsync([followUp.InspectionId], ct);
        if (inspection != null && followUp.DueDate.Date < inspection.InspectionDate.Date)
        {
            _logger.LogWarning("FollowUp create rejected: DueDate {DueDate} is before InspectionDate {InspectionDate}. InspectionId: {InspectionId}",
                followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);
            ModelState.AddModelError(nameof(FollowUp.DueDate), "Due date cannot be before the inspection date.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(followUp);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("FollowUp created. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}, Status: {Status}",
                followUp.Id, followUp.InspectionId, followUp.Status);
            return RedirectToAction(nameof(Index));
        }
        await PopulateInspections(ct, followUp.InspectionId);
        return View(followUp);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var f = await _context.FollowUps.Include(x => x.Inspection).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (f == null) return NotFound();
        await PopulateInspections(ct, f.InspectionId);
        return View(f);
    }

    [Authorize(Roles = "Admin,Inspector")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FollowUp followUp, CancellationToken ct)
    {
        if (id != followUp.Id) return NotFound();
        FollowUpEditValidation.ValidateClosedDateRequired(followUp, ModelState);

        var inspection = await _context.Inspections.FindAsync([followUp.InspectionId], ct);
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
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("FollowUp updated. FollowUpId: {FollowUpId}, Status: {Status}", followUp.Id, followUp.Status);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FollowUps.AnyAsync(e => e.Id == followUp.Id, ct)) return NotFound();
                throw;
            }
        }
        await PopulateInspections(ct, followUp.InspectionId);
        return View(followUp);
    }

    private async Task PopulateInspections(CancellationToken ct, int? selectedId = null)
    {
        var items = await _context.Inspections.Include(i => i.Premises).OrderByDescending(i => i.InspectionDate)
            .Select(i => new { i.Id, Display = "#" + i.Id + " - " + (i.Premises != null ? i.Premises.Name : "") + " - " + i.InspectionDate.ToString("d") }).ToListAsync(ct);
        ViewBag.InspectionId = new SelectList(items, "Id", "Display", selectedId);
    }
}

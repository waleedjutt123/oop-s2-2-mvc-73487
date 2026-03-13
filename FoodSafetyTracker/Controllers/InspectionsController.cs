using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector + "," + DbInitializer.RoleViewer)]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _context.Inspections
            .Include(i => i.Premises)
            .OrderByDescending(i => i.InspectionDate)
            .ToListAsync(cancellationToken);
        return View(list);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var inspection = await _context.Inspections.Include(i => i.Premises).Include(i => i.FollowUps)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (inspection == null) return NotFound();
        return View(inspection);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["PremisesId"] = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(cancellationToken), "Id", "Name");
        return View(new Inspection { InspectionDate = DateTime.Today });
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inspection inspection, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inspection);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Inspection created. PremisesId: {PremisesId}, InspectionId: {InspectionId}, Score: {Score}, Outcome: {Outcome}",
                inspection.PremisesId, inspection.Id, inspection.Score, inspection.Outcome);
            return RedirectToAction(nameof(Index));
        }
        ViewData["PremisesId"] = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(cancellationToken), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var inspection = await _context.Inspections.FindAsync([id], cancellationToken);
        if (inspection == null) return NotFound();
        ViewData["PremisesId"] = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(cancellationToken), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inspection inspection, CancellationToken cancellationToken)
    {
        if (id != inspection.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Inspection updated. InspectionId: {InspectionId}, Outcome: {Outcome}", inspection.Id, inspection.Outcome);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Inspections.AnyAsync(e => e.Id == inspection.Id, cancellationToken)) return NotFound();
                throw;
            }
        }
        ViewData["PremisesId"] = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(cancellationToken), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var inspection = await _context.Inspections.Include(i => i.Premises).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (inspection == null) return NotFound();
        return View(inspection);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections.FindAsync([id], cancellationToken);
        if (inspection != null)
        {
            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Inspection deleted. InspectionId: {InspectionId}", id);
        }
        return RedirectToAction(nameof(Index));
    }
}

using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _context.Inspections.Include(i => i.Premises).OrderByDescending(i => i.InspectionDate).ToListAsync(ct);
        return View(list);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var i = await _context.Inspections.Include(x => x.Premises).Include(x => x.FollowUps).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (i == null) return NotFound();
        return View(i);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.PremisesId = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(ct), "Id", "Name");
        return View(new Inspection { InspectionDate = DateTime.Today });
    }

    [Authorize(Roles = "Admin,Inspector")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inspection inspection, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inspection);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Inspection created. PremisesId: {PremisesId}, InspectionId: {InspectionId}, Score: {Score}, Outcome: {Outcome}",
                inspection.PremisesId, inspection.Id, inspection.Score, inspection.Outcome);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.PremisesId = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(ct), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var i = await _context.Inspections.FindAsync([id], ct);
        if (i == null) return NotFound();
        ViewBag.PremisesId = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(ct), "Id", "Name", i.PremisesId);
        return View(i);
    }

    [Authorize(Roles = "Admin,Inspector")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inspection inspection, CancellationToken ct)
    {
        if (id != inspection.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Inspection updated. InspectionId: {InspectionId}, Outcome: {Outcome}", inspection.Id, inspection.Outcome);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Inspections.AnyAsync(e => e.Id == inspection.Id, ct)) return NotFound();
                throw;
            }
        }
        ViewBag.PremisesId = new SelectList(await _context.Premises.OrderBy(p => p.Name).ToListAsync(ct), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Delete(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var i = await _context.Inspections.Include(x => x.Premises).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (i == null) return NotFound();
        return View(i);
    }

    [Authorize(Roles = "Admin,Inspector")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var i = await _context.Inspections.FindAsync([id], ct);
        if (i != null)
        {
            _context.Inspections.Remove(i);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Inspection deleted. InspectionId: {InspectionId}", id);
        }
        return RedirectToAction(nameof(Index));
    }
}

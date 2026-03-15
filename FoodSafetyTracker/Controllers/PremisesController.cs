using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class PremisesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PremisesController> _logger;

    public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _context.Premises.OrderBy(p => p.Town).ThenBy(p => p.Name).ToListAsync(ct);
        return View(list);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var p = await _context.Premises.Include(x => x.Inspections).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (p == null) return NotFound();
        return View(p);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new Premises());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Premises premises, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            _context.Add(premises);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Premises created. PremisesId: {PremisesId}, Name: {Name}", premises.Id, premises.Name);
            return RedirectToAction(nameof(Index));
        }
        return View(premises);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var p = await _context.Premises.FindAsync([id], ct);
        if (p == null) return NotFound();
        return View(p);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Premises premises, CancellationToken ct)
    {
        if (id != premises.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(premises);
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Premises updated. PremisesId: {PremisesId}, Name: {Name}", premises.Id, premises.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Premises.AnyAsync(e => e.Id == premises.Id, ct)) return NotFound();
                throw;
            }
        }
        return View(premises);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id, CancellationToken ct)
    {
        if (id == null) return NotFound();
        var p = await _context.Premises.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (p == null) return NotFound();
        return View(p);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var p = await _context.Premises.FindAsync([id], ct);
        if (p != null)
        {
            _context.Premises.Remove(p);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Premises deleted. PremisesId: {PremisesId}", id);
        }
        return RedirectToAction(nameof(Index));
    }
}

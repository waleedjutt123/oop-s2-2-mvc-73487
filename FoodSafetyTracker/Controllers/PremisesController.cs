using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector + "," + DbInitializer.RoleViewer)]
public class PremisesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PremisesController> _logger;

    public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _context.Premises.OrderBy(p => p.Town).ThenBy(p => p.Name).ToListAsync(cancellationToken);
        return View(list);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin + "," + DbInitializer.RoleInspector + "," + DbInitializer.RoleViewer)]
    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var premises = await _context.Premises.Include(p => p.Inspections).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (premises == null) return NotFound();
        return View(premises);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    [HttpGet]
    public IActionResult Create() => View(new Premises());

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Premises premises, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            _context.Add(premises);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Premises created. PremisesId: {PremisesId}, Name: {Name}", premises.Id, premises.Name);
            return RedirectToAction(nameof(Index));
        }
        return View(premises);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var premises = await _context.Premises.FindAsync([id], cancellationToken);
        if (premises == null) return NotFound();
        return View(premises);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Premises premises, CancellationToken cancellationToken)
    {
        if (id != premises.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(premises);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Premises updated. PremisesId: {PremisesId}, Name: {Name}", premises.Id, premises.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Premises.AnyAsync(e => e.Id == premises.Id, cancellationToken)) return NotFound();
                throw;
            }
        }
        return View(premises);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken)
    {
        if (id == null) return NotFound();
        var premises = await _context.Premises.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (premises == null) return NotFound();
        return View(premises);
    }

    [Authorize(Roles = DbInitializer.RoleAdmin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var premises = await _context.Premises.FindAsync([id], cancellationToken);
        if (premises != null)
        {
            _context.Premises.Remove(premises);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Premises deleted. PremisesId: {PremisesId}", id);
        }
        return RedirectToAction(nameof(Index));
    }
}

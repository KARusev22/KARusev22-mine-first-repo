using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Controllers;

public class AllergenController : Controller
{
    private readonly ApplicationDbContext _context;

    public AllergenController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var allergens = await _context.Allergens.ToListAsync();
        return View(allergens);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Allergen allergen)
    {
        if (!ModelState.IsValid)
            return View(allergen);

        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var allergen = await _context.Allergens.FindAsync(id);
        if (allergen == null)
            return NotFound();

        return View(allergen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Allergen allergen)
    {
        if (id != allergen.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(allergen);

        _context.Allergens.Update(allergen);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var allergen = await _context.Allergens.FindAsync(id);
        if (allergen == null)
            return NotFound();

        return View(allergen);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var allergen = await _context.Allergens.FindAsync(id);
        if (allergen != null)
        {
            _context.Allergens.Remove(allergen);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
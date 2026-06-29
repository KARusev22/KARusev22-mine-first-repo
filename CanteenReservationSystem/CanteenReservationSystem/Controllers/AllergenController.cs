using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Controllers;

public class AllergenController : Controller
{
    private readonly IAllergenService _allergenService;

    public AllergenController(IAllergenService allergenService)
    {
        _allergenService = allergenService;
    }

    public async Task<IActionResult> Index()
    {
        var allergens = await _allergenService.GetAllAsync();
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
        //Validate model before saving
        if (!ModelState.IsValid)
            return View(allergen);

        await _allergenService.CreateAsync(allergen);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var allergen = await _allergenService.GetByIdAsync(id);
        if (allergen == null)
            return NotFound();

        return View(allergen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Allergen allergen)
    {
        //Ensure the route ID matches the model ID
        if (id != allergen.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(allergen);
        
        await _allergenService.UpdateAsync(allergen);

        return RedirectToAction(nameof(Index));
    }

    //Shows confirmation page before deletion
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var allergen = await _allergenService.GetByIdAsync(id);
        if (allergen == null)
            return NotFound();

        return View(allergen);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _allergenService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
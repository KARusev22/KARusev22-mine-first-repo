using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanteenReservationSystem.Controllers;

public class KitchenController : Controller
{
    private readonly IKitchenService _kitchenService;
    private readonly IIngredientService _ingredientService;

    public KitchenController(IKitchenService kitchenService, IIngredientService ingredientService)
    {
        _kitchenService = kitchenService;
        _ingredientService = ingredientService;
    }

    public IActionResult Index(DateTime? date)
    {
        ViewData["FigustaNav"] = "kitchenDashboard";
        
        //Default to today's date if none is provided
        var selectedDate = date ?? DateTime.Today;
        
        //Retrieve kitchen data
        var vm = _kitchenService.GetKitchenData(selectedDate);
        return View(vm);
    }

    //Inventory editor: update the available stock (grams) for an ingredient.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int ingredientId, int availableGrams)
    {
        var ingredient = await _ingredientService.GetByIdAsync(ingredientId);
        if (ingredient == null)
            return NotFound();

        ingredient.AvailableGrams = Math.Max(0, availableGrams);
        await _ingredientService.UpdateAsync(ingredient);

        return Json(new { ok = true, ingredientId, availableGrams = ingredient.AvailableGrams });
    }
}
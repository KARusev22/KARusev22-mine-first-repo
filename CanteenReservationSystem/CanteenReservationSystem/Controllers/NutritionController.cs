using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Controllers;

public class NutritionController : Controller
{
    private readonly INutritionService _nutritionService;

    public NutritionController(INutritionService nutritionService)
    {
        _nutritionService = nutritionService;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int dishId)
    {
        var nutrition = await _nutritionService.GetByDishIdAsync(dishId);

        //Create a new nutrition entry if none exists
        if (nutrition == null)
        {
            nutrition = new Nutrition { DishId = dishId };
        }

        return View(nutrition);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Nutrition nutrition)
    {
        if (!ModelState.IsValid)
            return View(nutrition);

        if (nutrition.Id == 0)
            await _nutritionService.CreateAsync(nutrition);
        else
            await _nutritionService.UpdateAsync(nutrition);

        //Redirect back to the dish details page
        return RedirectToAction("Details", "Dish", new { id = nutrition.DishId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int dishId)
    {
        await _nutritionService.DeleteByDishIdAsync(dishId);
        
        //Redirect back to the dish details page
        return RedirectToAction("Details", "Dish", new { id = dishId });
    }
}
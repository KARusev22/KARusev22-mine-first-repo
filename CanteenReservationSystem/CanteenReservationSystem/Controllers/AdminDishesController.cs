using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models.ViewModels;
using Ganss.Xss;

namespace CanteenReservationSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDishesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDishService _dishService;
    private readonly IIngredientService _ingredientService;
    private readonly IWebHostEnvironment _env;
    private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public AdminDishesController(
        ApplicationDbContext context,
        IDishService dishService,
        IIngredientService ingredientService,
        IWebHostEnvironment env)
    {
        _context = context;
        _dishService = dishService;
        _ingredientService = ingredientService;
        _env = env;

        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
    }

    public async Task<IActionResult> Index()
    {
        var dishes = await _dishService.GetAllAsync();
        return View(dishes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new DishFormViewModel
        {
            Dish = new Dish(),
            Nutrition = new Nutrition(),
            Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName })
                .ToListAsync(),
            Allergens = await _context.Allergens.ToListAsync()
        };

        return View(vm);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(DishFormViewModel vm, IFormFile? imageFile)
{
    if (imageFile == null)
        ModelState.AddModelError("Dish.ImageUrl", "Image is required.");

    if (!ModelState.IsValid)
    {
        vm.Categories = await _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName })
            .ToListAsync();

        vm.Allergens = await _context.Allergens.ToListAsync();

        vm.Nutrition ??= new Nutrition();
        vm.IngredientIds ??= new();
        vm.IngredientNames ??= new();
        vm.IngredientGrams ??= new();
        vm.SelectedAllergenIds ??= new();

        return View(vm);
    }

    vm.Dish.Description = _sanitizer.Sanitize(vm.Dish.Description);

    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
    var path = Path.Combine(_env.WebRootPath, "images", fileName);
    using (var stream = new FileStream(path, FileMode.Create))
        await imageFile.CopyToAsync(stream);

    vm.Dish.ImageUrl = "/images/" + fileName;

    await _dishService.CreateAsync(vm.Dish);

    vm.Nutrition.DishId = vm.Dish.Id;
    _context.Nutritions.Add(vm.Nutrition);

    for (int i = 0; i < vm.IngredientNames.Count; i++)
    {
        if (string.IsNullOrWhiteSpace(vm.IngredientNames[i]))
            continue;

        var ingredient = await _ingredientService.FindOrCreateByNameAsync(vm.IngredientNames[i]);

        _context.DishIngredients.Add(new DishIngredient
        {
            DishId = vm.Dish.Id,
            IngredientId = ingredient.Id,
            GramsPerPortion = vm.IngredientGrams[i]
        });
    }

    if (vm.SelectedAllergenIds != null)
    {
        foreach (var allergenId in vm.SelectedAllergenIds)
        {
            _context.DishAllergens.Add(new DishAllergen
            {
                DishId = vm.Dish.Id,
                AllergenId = allergenId
            });
        }
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}

    public async Task<IActionResult> Edit(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        var nutrition = await _context.Nutritions
            .FirstOrDefaultAsync(n => n.DishId == id);

        var ingredients = await _context.DishIngredients
            .Where(di => di.DishId == id)
            .Include(di => di.Ingredient)
            .ToListAsync();

        var allergens = await _context.Allergens.ToListAsync();

        var selectedAllergens = await _context.DishAllergens
            .Where(da => da.DishId == id)
            .Select(da => da.AllergenId)
            .ToListAsync();

        var categories = await _context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            })
            .ToListAsync();

        var vm = new DishFormViewModel
        {
            Dish = dish,
            Nutrition = nutrition,
            Categories = categories,

            IngredientIds = ingredients.Select(i => i.Id).ToList(), // DishIngredientId
            IngredientNames = ingredients.Select(i => i.Ingredient.IngredientName).ToList(),
            IngredientGrams = ingredients.Select(i => i.GramsPerPortion).ToList(),

            Allergens = allergens,
            SelectedAllergenIds = selectedAllergens
        };

        ViewData["FigustaPage"] = true;
        return View(vm);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, DishFormViewModel vm, IFormFile? imageFile)
{
    if (id != vm.Dish.Id)
        return BadRequest();

    var existingDish = await _dishService.GetByIdAsync(id);
    if (existingDish == null)
        return NotFound();

    if (imageFile != null)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        var path = Path.Combine(_env.WebRootPath, "images", fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        vm.Dish.ImageUrl = "/images/" + fileName;
    }
    else
    {
        vm.Dish.ImageUrl = existingDish.ImageUrl;
    }
    
    vm.Dish.Description = _sanitizer.Sanitize(vm.Dish.Description);

    _context.Entry(existingDish).CurrentValues.SetValues(vm.Dish);

    var existingNutrition = await _context.Nutritions
        .FirstOrDefaultAsync(n => n.DishId == id);

    if (existingNutrition == null)
    {
        vm.Nutrition.DishId = id;
        _context.Nutritions.Add(vm.Nutrition);
    }
    else
    {
        existingNutrition.WeightGrams = vm.Nutrition.WeightGrams;
        existingNutrition.Calories = vm.Nutrition.Calories;
        existingNutrition.Protein = vm.Nutrition.Protein;
        existingNutrition.Fats = vm.Nutrition.Fats;
        existingNutrition.Carbohydrates = vm.Nutrition.Carbohydrates;
        existingNutrition.Fiber = vm.Nutrition.Fiber;
    }

    var existingIngredients = await _context.DishIngredients
        .Where(di => di.DishId == id)
        .ToListAsync();

    var formDishIngredientIds = vm.IngredientIds.Where(x => x > 0).ToList();

    var toDelete = existingIngredients
        .Where(di => !formDishIngredientIds.Contains(di.Id))
        .ToList();

    _context.DishIngredients.RemoveRange(toDelete);

    for (int i = 0; i < vm.IngredientNames.Count; i++)
    {
        var name = vm.IngredientNames[i];
        var grams = vm.IngredientGrams[i];
        var diId = vm.IngredientIds[i];

        if (string.IsNullOrWhiteSpace(name))
            continue;

        var ingredient = await _ingredientService.FindOrCreateByNameAsync(name);

        if (diId == 0)
        {
            _context.DishIngredients.Add(new DishIngredient
            {
                DishId = id,
                IngredientId = ingredient.Id,
                GramsPerPortion = grams
            });
        }
        else
        {
            var existing = existingIngredients.First(di => di.Id == diId);
            existing.IngredientId = ingredient.Id;
            existing.GramsPerPortion = grams;
        }
    }
    
    var existingAllergens = await _context.DishAllergens
        .Where(da => da.DishId == id)
        .ToListAsync();

    var selected = vm.SelectedAllergenIds ?? new List<int>();

    foreach (var allergenId in selected)
    {
        if (!existingAllergens.Any(a => a.AllergenId == allergenId))
        {
            _context.DishAllergens.Add(new DishAllergen
            {
                DishId = id,
                AllergenId = allergenId
            });
        }
    }

    foreach (var allergen in existingAllergens)
    {
        if (!selected.Contains(allergen.AllergenId))
            _context.DishAllergens.Remove(allergen);
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _dishService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
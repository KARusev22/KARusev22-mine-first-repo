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

//Restricts access
[Authorize(Roles = "Admin")]
public class AdminDishesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDishService _dishService;
    private readonly IIngredientService _ingredientService;
    private readonly IWebHostEnvironment _env;
    
    //HTML sanitizer to prevent XSS attacks
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

        //Allow basic formatting tags
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
    }

    //Displays all dishes
    public async Task<IActionResult> Index()
    {
        var dishes = await _dishService.GetAllAsync();
        return View(dishes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        //Prepare form view model with empty dish and nutrition objects
        var vm = new DishFormViewModel
        {
            Dish = new Dish(),
            Nutrition = new Nutrition(),
            
            //Load categories and allergens
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
    //Image is mandatory
    if (imageFile == null)
        ModelState.AddModelError("Dish.ImageUrl", "Image is required.");

    //If fails, reload lists and return form
    if (!ModelState.IsValid)
    {
        vm.Categories = await _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName })
            .ToListAsync();

        vm.Allergens = await _context.Allergens.ToListAsync();

        //Ensure collections are initialized to avoid null
        vm.Nutrition ??= new Nutrition();
        vm.IngredientIds ??= new();
        vm.IngredientNames ??= new();
        vm.IngredientGrams ??= new();
        vm.SelectedAllergenIds ??= new();

        return View(vm);
    }

    
    //Sanitize description to prevent XSS
    vm.Dish.Description = _sanitizer.Sanitize(vm.Dish.Description);

    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
    var path = Path.Combine(_env.WebRootPath, "images", fileName);
    using (var stream = new FileStream(path, FileMode.Create))
        await imageFile.CopyToAsync(stream);

    vm.Dish.ImageUrl = "/images/" + fileName;

    //Create dish record
    await _dishService.CreateAsync(vm.Dish);

    //Save nutrition info
    vm.Nutrition.DishId = vm.Dish.Id;
    _context.Nutritions.Add(vm.Nutrition);

    //Ingredient relations
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

    //Save allergen relations
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
        //Load dish and related data
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

        //Build view model for editing
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

        //Used for UI logic
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

    //Handle optional image update
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
    
    //Sanitize description
    vm.Dish.Description = _sanitizer.Sanitize(vm.Dish.Description);

    //Update dish entity values
    _context.Entry(existingDish).CurrentValues.SetValues(vm.Dish);

    //Update or create nutrition
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

    //Determine which ingredients were removed
    var formDishIngredientIds = vm.IngredientIds.Where(x => x > 0).ToList();

    var toDelete = existingIngredients
        .Where(di => !formDishIngredientIds.Contains(di.Id))
        .ToList();

    _context.DishIngredients.RemoveRange(toDelete);

    //Update or add ingredients
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
            //New ingredient relation
            _context.DishIngredients.Add(new DishIngredient
            {
                DishId = id,
                IngredientId = ingredient.Id,
                GramsPerPortion = grams
            });
        }
        else
        {
            //Update existing relation
            var existing = existingIngredients.First(di => di.Id == diId);
            existing.IngredientId = ingredient.Id;
            existing.GramsPerPortion = grams;
        }
    }
    
    var existingAllergens = await _context.DishAllergens
        .Where(da => da.DishId == id)
        .ToListAsync();

    var selected = vm.SelectedAllergenIds ?? new List<int>();

    //Add new allergens
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

    //Remove unselected allergens
    foreach (var allergen in existingAllergens)
    {
        if (!selected.Contains(allergen.AllergenId))
            _context.DishAllergens.Remove(allergen);
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}

    // Soft delete dish
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _dishService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Deleted()
    {
        var deleted = await _dishService.GetDeletedAsync();
        return View(deleted);
    }
    
    [HttpPost]
    public async Task<IActionResult> Restore(int id)
    {
        await _dishService.RestoreAsync(id);
        return RedirectToAction("Deleted");
    }
}
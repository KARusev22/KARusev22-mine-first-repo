using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Services;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanteenReservationSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Ganss.Xss;
 
namespace CanteenReservationSystem.Controllers;

public class DishController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env; //Access to wwwroot for file uploads
    private readonly IDishService _dishService;
    private readonly IIngredientService _ingredientService; 
    private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public DishController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env, IDishService dishService, IIngredientService ingredientService)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
        _dishService = dishService;
        _ingredientService = ingredientService;
        
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
    
    public async Task<IActionResult> Index()
    {
        var dishes = await _dishService.GetAllAsync();
        return View(dishes);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dish dish, IFormFile? imageFile,
                                            List<string> ingredientNames,List<int> ingredientGrams)
    {
        if (!ModelState.IsValid)
            return View(dish);

        //Sanitize description
        dish.Description = _sanitizer.Sanitize(dish.Description);

        //Upload image if provided
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            dish.ImageUrl = "/uploads/" + fileName;
        }

        //Save dish record
        await _dishService.CreateAsync(dish);

        //Create ingredient relations
        for (int i = 0; i < ingredientNames.Count; i++)
        {
            var name = ingredientNames[i];
            var grams = ingredientGrams[i];

            if (string.IsNullOrWhiteSpace(name))
                continue;

            var ingredient = await _ingredientService.FindOrCreateByNameAsync(name);

            _context.DishIngredients.Add(new DishIngredient
            {
                DishId = dish.Id,
                IngredientId = ingredient.Id,
                GramsPerPortion = grams
            });
        }
        
        //Save allergen relations
        if (Request.Form.TryGetValue("selectedAllergens", out var allergenValues))
        {
            foreach (var allergenIdStr in allergenValues)
            {
                if (int.TryParse(allergenIdStr, out int allergenId))
                {
                    _context.DishAllergens.Add(new DishAllergen
                    {
                        DishId = dish.Id,
                        AllergenId = allergenId
                    });
                }
            }
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();
        
        var today = DateTime.Today;

        //Determine which days this dish is available in the current month
        var availableDays = await _context.MonthlyMenu
            .Where(m => m.DishId == id &&
                        m.Month == today.Month &&
                        m.Year == today.Year)
            .Select(m => m.DayOfWeek)
            .ToListAsync();

        ViewBag.AvailableDays = availableDays;

        return View(dish);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        var allAllergens = await _context.Allergens.ToListAsync();
        var selectedAllergenIds = await _context.DishAllergens
            .Where(da => da.DishId == dish.Id)
            .Select(da => da.AllergenId)
            .ToListAsync();

        ViewBag.Allergens = allAllergens;
        ViewBag.SelectedAllergens = selectedAllergenIds;
        
        return View(dish);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Dish dish, IFormFile? imageFile, List<int> ingredientIds,
                                        List<string> ingredientNames, List<int> ingredientGrams)
    {
        if (id != dish.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dish);

        //Sanitize description
        dish.Description = _sanitizer.Sanitize(dish.Description);

        //Upload new image if provided
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            dish.ImageUrl = "/uploads/" + fileName;
        }

        await _dishService.UpdateAsync(dish);

        var existing = await _context.DishIngredients
            .Where(di => di.DishId == dish.Id)
            .ToListAsync();
        
        //Update or add ingredients
        for (int i = 0; i < ingredientNames.Count; i++)
        {
            var ingId = ingredientIds[i];
            var name = ingredientNames[i];
            var grams = ingredientGrams[i];

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var ingredient = await _ingredientService.FindOrCreateByNameAsync(name);

            if (ingId > 0)
            {
                //Update existing ingredient relation
                var existingRecord = existing.First(di => di.Id == ingId);

                existingRecord.IngredientId = ingredient.Id;
                existingRecord.GramsPerPortion = grams;

                _context.DishIngredients.Update(existingRecord);
            }
            else
            {
                //Add new ingredient relation
                _context.DishIngredients.Add(new DishIngredient
                {
                    DishId = dish.Id,
                    IngredientId = ingredient.Id,
                    GramsPerPortion = grams
                });
            }
        }
        
        //Remove ingredients that were deleted in the form
        var idsFromForm = ingredientIds.Where(x => x > 0).ToList();

        var toDelete = existing
            .Where(di => !idsFromForm.Contains(di.Id))
            .ToList();

        _context.DishIngredients.RemoveRange(toDelete);
        
        var existingAllergens = await _context.DishAllergens
            .Where(da => da.DishId == dish.Id)
            .ToListAsync();

        var selectedAllergens = Request.Form["selectedAllergens"]
            .Select(int.Parse)
            .ToList();
     
        foreach (var allergenId in selectedAllergens)
        {
            if (!existingAllergens.Any(da => da.AllergenId == allergenId))
            {
                _context.DishAllergens.Add(new DishAllergen
                {
                    DishId = dish.Id,
                    AllergenId = allergenId
                });
            }
        }
        
        //Remove unselected allergens
        foreach (var allergenRecord  in existingAllergens)
        {
            if (!selectedAllergens.Contains(allergenRecord .AllergenId))
            {
                _context.DishAllergens.Remove(allergenRecord );
            }
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    //Soft delete dish
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _dishService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

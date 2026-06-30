using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Controllers;

public class AdminMonthlyMenuController : Controller
{
    private readonly ApplicationDbContext _context;

    //Inject the EF Core database context
    public AdminMonthlyMenuController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        //Load all dishes
        ViewBag.Dishes = await _context.Dishes
            .Include(d => d.Category)
            .OrderBy(d => d.Category.CategoryName)
            .ToListAsync();

        //Pre-fill the form with the current month and year
        return View(new MonthlyMenu
        {
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MonthlyMenu model)
    {
        //Basic validations
        if (model.DishId == null)
        {
            ModelState.AddModelError("DishId", "Please select a dish.");
        }
        
        if (model.Year < 2026)
            ModelState.AddModelError("Year", "Year cannot be earlier than 2026.");

        if (model.Month < 1 || model.Month > 12)
            ModelState.AddModelError("Month", "Month must be between 1 and 12.");

        var dish = await _context.Dishes
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == model.DishId);

        if (dish == null)
        {
            ModelState.AddModelError("", "Invalid dish.");
        }
        else
        {
            //Each category can appear max 2 times per day
            int categoryId = dish.CategoryId;

            int countForCategory = await _context.MonthlyMenu
                .Include(m => m.Dish)
                .Where(m =>
                    m.Month == model.Month &&
                    m.Year == model.Year &&
                    m.DayOfWeek == model.DayOfWeek &&
                    m.Dish.CategoryId == categoryId)
                .CountAsync();

            if (countForCategory >= 2)
            {
                ModelState.AddModelError("",
                    $"This category already has 2 dishes for {model.DayOfWeek}.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Dishes = await _context.Dishes
                .Include(d => d.Category)
                .OrderBy(d => d.Category.CategoryName)
                .ToListAsync();

            return View(model);
        }

        //Save the new monthly menu entry
        _context.MonthlyMenu.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        //Load all monthly menu entries
        var menus = await _context.MonthlyMenu
            .Include(m => m.Dish)
            .ThenInclude(d => d.Category)
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ThenBy(m => m.DayOfWeek)
            .ToListAsync();

        return View(menus);
    }
}
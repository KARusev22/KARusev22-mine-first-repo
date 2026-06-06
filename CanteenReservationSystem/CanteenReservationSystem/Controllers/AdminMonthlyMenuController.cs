using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Controllers;

public class AdminMonthlyMenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminMonthlyMenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        ViewBag.Dishes = _context.Dishes.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Create(int month, int year, List<DayOfWeek> days, List<int> dishIds)
    {
        if (days.Count != 7 || dishIds.Count != 14)
        {
            ModelState.AddModelError("", "Invalid menu data.");
            ViewBag.Dishes = _context.Dishes.ToList();
            return View();
        }

        for (int i = 0; i < days.Count; i++)
        {
            var day = days[i];

            var dish1 = dishIds[i * 2];
            var dish2 = dishIds[i * 2 + 1];

            _context.MonthlyMenu.Add(new MonthlyMenu
            {
                DishId = dish1,
                DayOfWeek = day,
                Month = month,
                Year = year
            });

            _context.MonthlyMenu.Add(new MonthlyMenu
            {
                DishId = dish2,
                DayOfWeek = day,
                Month = month,
                Year = year
            });
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        var menus = _context.MonthlyMenu
            .Include(m => m.Dish)
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ThenBy(m => m.DayOfWeek)
            .ToList();

        return View(menus);
    }
}
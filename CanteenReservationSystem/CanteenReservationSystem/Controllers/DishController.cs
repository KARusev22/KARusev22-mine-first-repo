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
    private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public DishController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env, IDishService dishService)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
        _dishService = dishService;
        
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
    public async Task<IActionResult> Create(Dish dish, IFormFile? imageFile)
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

        await _dishService.CreateAsync(dish);

        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        return View(dish);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        return View(dish);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Dish dish, IFormFile? imageFile)
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

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        return View(dish);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _dishService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

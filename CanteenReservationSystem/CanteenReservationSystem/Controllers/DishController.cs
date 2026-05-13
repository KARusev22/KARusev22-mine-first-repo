using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Services;
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
    private readonly UserManager<ApplicationDbContext> _userManager;
    private readonly IWebHostEnvironment _env; //Access to wwwroot for file uploads
    private readonly DishService _dishService;
    private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public DishController(ApplicationDbContext context, UserManager<ApplicationDbContext> userManager,
        IWebHostEnvironment env, DishService dishService)
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
    
    
}
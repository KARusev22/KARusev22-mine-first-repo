using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models.ViewModels;

namespace CanteenReservationSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status,
        DateTime? date,
        string? code)
    {
        ViewData["FigustaPage"] = true;
        
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails).ThenInclude(od => od.Dish)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);
        
        if (date.HasValue)
            query = query.Where(o => o.TargetDate.Date == date.Value.Date);
        
        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(o => o.UniqueCode.Contains(code));
        
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        
        var model = orders.Select(o => new OrderViewModel
        {
            Id = o.Id,
            Code = o.UniqueCode,
            CreatedOn = o.CreatedAt,
            TargetDate = o.TargetDate,
            Status = o.Status,
            Notes = o.OrderDetails.Any(d => d.Note != null)
                ? string.Join("; ", o.OrderDetails.Where(d => d.Note != null).Select(d => d.Note))
                : null,
            TotalPrice = o.TotalPrice,
            Items = o.OrderDetails.Select(d => new OrderItemViewModel
            {
                Name = d.Dish.DishName,
                Quantity = d.Quantity
            }).ToList()
        }).ToList();

        ViewBag.UserNames = orders.ToDictionary(
            o => o.Id,
            o => o.User.FullName
        );

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["FigustaPage"] = true;
        
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        var model = new OrderViewModel
        {
            Id = order.Id,
            Code = order.UniqueCode,
            CreatedOn = order.CreatedAt,
            TargetDate = order.TargetDate,
            Status = order.Status,
            Notes = order.OrderDetails.Any(d => d.Note != null)
                ? string.Join("; ", order.OrderDetails.Where(d => d.Note != null).Select(d => d.Note))
                : null,
            TotalPrice = order.TotalPrice,
            Items = order.OrderDetails.Select(d => new OrderItemViewModel
            {
                Name = d.Dish.DishName,
                Quantity = d.Quantity
            }).ToList()
        };

        ViewBag.UserFullName = order.User.FullName;

        return View(model);
    }
}
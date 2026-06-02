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
    
    public async Task<IActionResult> MarkNotClaimed(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        order.Status = "NotClaimed";
        order.User.BlackPoints += 1;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Dashboard() 
    {
        ViewData["FigustaPage"] = true;

        var today = DateTime.Today;

        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .Include(o => o.User)
            .ToListAsync();

        var totalOrders = orders.Count;
        var ordersToday = orders.Count(o => o.TargetDate.Date == today);
        var totalRevenue = orders.Sum(o => o.TotalPrice);
        var todayRevenue = orders.Where(o => o.TargetDate.Date == today).Sum(o => o.TotalPrice);

        var groupedByDay = orders
            .GroupBy(o => o.TargetDate.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var ordersPerDayLabels = groupedByDay.Select(g => g.Key.ToString("dd MMM")).ToList();
        var ordersPerDayValues = groupedByDay.Select(g => g.Count()).ToList();

        var statusLabels = new List<string> { "Pending", "Completed", "NotClaimed" };
        var statusValues = new List<int>
        {
            orders.Count(o => o.Status == "Pending"),
            orders.Count(o => o.Status == "Completed"),
            orders.Count(o => o.Status == "NotClaimed")
        };

        var topDishes = orders
            .SelectMany(o => o.OrderDetails)
            .GroupBy(d => d.Dish.DishName)
            .OrderByDescending(g => g.Sum(x => x.Quantity))
            .Take(7)
            .ToList();

        var topDishesLabels = topDishes.Select(g => g.Key).ToList();
        var topDishesValues = topDishes.Select(g => g.Sum(x => x.Quantity)).ToList();

        var revenuePerDayLabels = groupedByDay.Select(g => g.Key.ToString("dd MMM")).ToList();
        var revenuePerDayValues = groupedByDay.Select(g => g.Sum(o => o.TotalPrice)).ToList();

        var usersWithPoints = orders
            .Select(o => o.User)
            .Where(u => u.BlackPoints > 0)
            .GroupBy(u => u.FullName)
            .Select(g => new { Name = g.Key, Points = g.Sum(x => x.BlackPoints) })
            .OrderByDescending(x => x.Points)
            .Take(7)
            .ToList();

        var blackPointsLabels = usersWithPoints.Select(x => x.Name).ToList();
        var blackPointsValues = usersWithPoints.Select(x => x.Points).ToList();

        var activeUsers = orders
            .GroupBy(o => o.User.FullName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(7)
            .ToList();

        var activeUsersLabels = activeUsers.Select(x => x.Name).ToList();
        var activeUsersValues = activeUsers.Select(x => x.Count).ToList();
        
        var model = new AdminDashboardViewModel
        {
            TotalOrders = totalOrders,
            OrdersToday = ordersToday,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,

            OrdersPerDay_Labels = ordersPerDayLabels,
            OrdersPerDay_Values = ordersPerDayValues,

            StatusLabels = statusLabels,
            StatusValues = statusValues,

            TopDishes_Labels = topDishesLabels,
            TopDishes_Values = topDishesValues,

            RevenuePerDay_Labels = revenuePerDayLabels,
            RevenuePerDay_Values = revenuePerDayValues,

            BlackPoints_Labels = blackPointsLabels,
            BlackPoints_Values = blackPointsValues,

            ActiveUsers_Labels = activeUsersLabels,
            ActiveUsers_Values = activeUsersValues
        };

        return View(model);
    }
    
}
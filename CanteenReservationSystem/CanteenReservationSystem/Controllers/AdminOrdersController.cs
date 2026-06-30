using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    
    //Inject EF Core database context
    public AdminOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    //Displays all orders with optional filtering
    public async Task<IActionResult> Index(string? status,
        DateTime? date,
        string? code)
    {
        
        ViewData["FigustaPage"] = true;
        
        //IgnoreQueryFilters() ensures soft-deleted dishes still appear
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails).ThenInclude(od => od.Dish).IgnoreQueryFilters()
            .AsQueryable();
        
        //Apply optional filters
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);
        
        if (date.HasValue)
            query = query.Where(o => o.TargetDate.Date == date.Value.Date);
        
        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(o => o.UniqueCode.Contains(code));
        
        //Sort newest orders first
        var orders = await query
            .OrderByDescending(o => o.TargetDate)
            .ToListAsync();
        
        //Map database entities to view model
        var model = orders.Select(o => new OrderViewModel
        {
            Id = o.Id,
            Code = o.UniqueCode,
            CreatedOn = o.CreatedAt,
            TargetDate = o.TargetDate,
            Status = o.Status,
            
            // Combine all notes into a single string
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

        //Provide usernames for display in the view
        ViewBag.UserNames = orders.ToDictionary(
            o => o.Id,
            o => o.User.FullName
        );

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["FigustaPage"] = true;
        
        //Load order with related user and dish data
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish).IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        //Build view model for detailed view
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
    
    public async Task<IActionResult> MarkNotTaken(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        //Update status and penalize user
        order.Status = "NotTaken";
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

        //Basic metrics
        var totalOrders = orders.Count;
        var ordersToday = orders.Count(o => o.TargetDate.Date == today);
        var totalRevenue = orders.Sum(o => o.TotalPrice);
        var todayRevenue = orders.Where(o => o.TargetDate.Date == today).Sum(o => o.TotalPrice);

        //Group orders by day for charts
        var groupedByDay = orders
            .GroupBy(o => o.TargetDate.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var ordersPerDayLabels = groupedByDay.Select(g => g.Key.ToString("dd MMM")).ToList();
        var ordersPerDayValues = groupedByDay.Select(g => g.Count()).ToList();

        //Status distribution
        var statusLabels = new List<string> { "Pending", "Completed", "NotTaken" };
        var statusValues = new List<int>
        {
            orders.Count(o => o.Status == "Pending"),
            orders.Count(o => o.Status == "Completed"),
            orders.Count(o => o.Status == "NotTaken")
        };

        var topDishes = orders
            .SelectMany(o => o.OrderDetails)
            .GroupBy(d => d.Dish.DishName)
            .OrderByDescending(g => g.Sum(x => x.Quantity))
            .Take(7)
            .ToList();

        var topDishesLabels = topDishes.Select(g => g.Key).ToList();
        var topDishesValues = topDishes.Select(g => g.Sum(x => x.Quantity)).ToList();

        //Revenue
        var revenuePerDayLabels = groupedByDay.Select(g => g.Key.ToString("dd MMM")).ToList();
        var revenuePerDayValues = groupedByDay.Select(g => g.Sum(o => o.TotalPrice)).ToList();

        //Black points
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
        
        //Build dashboard view model
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
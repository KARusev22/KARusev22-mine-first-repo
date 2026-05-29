using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Orders>> GetOrdersByUserAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserStatsViewModel> GetUserStatsAsync(string userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .ThenInclude(d => d.Category)
            .ToListAsync();

        if (!orders.Any())
            return new UserStatsViewModel();

        var details = orders.SelectMany(o => o.OrderDetails);

        return new UserStatsViewModel
        {
            TotalOrders = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalPrice),
            MostOrderedDish = details
                .GroupBy(d => d.Dish.DishName)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => g.Key)
                .FirstOrDefault(),

            TakenOrders = orders.Count(o => o.Status == "Completed" || o.Status == "Delivered" || o.Status == "PickedUp"),
            NotTakenOrders = orders.Count(o => o.Status == "Pending" || o.Status == "Cancelled"),

            Top3Days = orders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToDictionary(
                    g => g.Key.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    g => g.Count()
                ),

            Top3Categories = details
                .GroupBy(d => d.Dish.Category.CategoryName)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Take(3)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity))
        };
    }
}
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
            .ToListAsync();

        return new UserStatsViewModel
        {
            TotalOrders = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalPrice),
            MostOrderedDish = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(d => d.Dish.DishName)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => g.Key)
                .FirstOrDefault()
        };
    }
}
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
    
    public async Task<Orders> GetByIdForUserAsync(int id, string userId)
    {
        return await _context.Orders
            .Where(o => o.Id == id && o.UserId == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Dish)
            .FirstOrDefaultAsync();
    }
    
    public async Task DeleteAsync(Orders order)
    {
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Orders order, EditOrderViewModel model)
    {
        if (model.TargetDate.Date <= DateTime.Today)
            throw new InvalidOperationException("Cannot set a past date.");

        var existing = await _context.OrderDetails
            .Where(od => od.OrderId == order.Id)
            .ToListAsync();

        _context.OrderDetails.RemoveRange(existing);

        decimal total = 0;

        foreach (var item in model.Items)
        {
            var dish = await _context.Dishes.FindAsync(item.DishId);

            _context.OrderDetails.Add(new OrderDetails
            {
                OrderId = order.Id,
                DishId = item.DishId,
                Quantity = item.Quantity,
                Note = item.Note
            });

            total += dish.Price * item.Quantity;
        }
        
        order.TotalPrice = total;
        order.TargetDate = model.TargetDate;

        await _context.SaveChangesAsync();
    }
}
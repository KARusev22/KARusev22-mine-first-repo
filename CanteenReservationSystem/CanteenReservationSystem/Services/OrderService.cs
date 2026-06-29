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

            TakenOrders = orders.Count(o => o.Status == "Completed"),
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
    
    //Updates an order based on the edit form model.
    public async Task<string?> UpdateAsync(Orders order, EditOrderViewModel model)
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

            //Check if dish is available on the selected date
            bool isAvailable = await _context.MonthlyMenu.AnyAsync(m =>
                m.DishId == item.DishId &&
                m.Month == model.TargetDate.Month &&
                m.Year == model.TargetDate.Year &&
                m.DayOfWeek == model.TargetDate.DayOfWeek);

            if (!isAvailable)
            {
                return $"The dish '{dish.DishName}' is not available on {model.TargetDate:dddd}.";
            }
           
            item.Price = dish.Price;
            
            //Add updated order detail
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
        return null;
    }
    
    //Retrieves an order by its unique code
    public async Task<Orders?> GetByUniqueCodeAsync(string code)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Dish)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UniqueCode == code);
    }

    public async Task MarkAsCompletedAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return;

        order.Status = "Completed";

        await _context.SaveChangesAsync();
    }

    public async Task MarkAllPendingAsNotTakenAsync()
    {
        var today = DateTime.Today;

        var orders = await _context.Orders
            .Include(o => o.User)
            .Where(o => o.Status == "Pending" && o.TargetDate.Date < today)
            .ToListAsync();

        foreach (var order in orders)
        {
            order.Status = "NotTaken";
            order.User.BlackPoints += 1;
        }

        await _context.SaveChangesAsync();
    }
    
    public async Task<Orders?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
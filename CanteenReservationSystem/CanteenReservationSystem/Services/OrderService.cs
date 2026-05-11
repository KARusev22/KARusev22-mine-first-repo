using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<IEnumerable<Orders>> GetUserOrdersAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Orders?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Orders> CreateOrderAsync(string userId, DateTime targetDate)
    {
        var cartItems = await _cartService.GetUserCartAsync(userId);

        var itemsForDate = cartItems
            .Where(c => c.TargetDate.Date == targetDate.Date)
            .ToList();

        if (!itemsForDate.Any())
            throw new InvalidOperationException("No items in cart for this date.");

        var order = new Orders
        {
            UserId = userId,
            UniqueCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = itemsForDate.Sum(i => i.Dish.Price * i.Quantity),
            OrderDetails = itemsForDate.Select(c => new OrderDetails
            {
                DishId = c.DishId,
                Quantity = c.Quantity,
                Note = c.Note
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _cartService.ClearCartAsync(userId);

        return order;
    }

    public async Task CancelOrderAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order != null)
        {
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Payments.RemoveRange(order.Payments);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
        }
    }
}
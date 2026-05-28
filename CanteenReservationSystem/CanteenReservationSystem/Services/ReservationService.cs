using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
namespace CanteenReservationSystem.Services;

public class ReservationService : IReservationService
{
   private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public ReservationService(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<Orders> CreateReservationAsync(string userId, DateTime targetDate, List<int> selectedItemIds)
    {
        var selectedItems = await _cartService.GetItemsByIdsAsync(selectedItemIds);

        if (!selectedItems.Any())
            throw new InvalidOperationException("You must select at least one item");

        var minDate = DateTime.Today.AddDays(1);

        if (targetDate.Date < minDate)
            throw new InvalidOperationException("You can order for tomorrow at the earliest.");

        var reservation = new Orders
        {
            UserId = userId,
            UniqueCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            TotalPrice = selectedItems.Sum(i => i.Dish.Price * i.Quantity),
            OrderDetails = selectedItems.Select(i => new OrderDetails
            {
                DishId = i.DishId,
                Quantity = i.Quantity,
                Note = i.Note
            }).ToList()
        };

        _context.Orders.Add(reservation);
        await _context.SaveChangesAsync();

        await _cartService.RemoveItemsByIdsAsync(selectedItemIds);

        return reservation;
    }

    public async Task<Orders?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Orders?> GetByCodeAsync(string code)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.UniqueCode == code);
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var reservation = await _context.Orders.FindAsync(id);

        if (reservation == null)
            return;

        reservation.Status = status;
        await _context.SaveChangesAsync();
    } 
}

using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItems>> GetUserCartAsync(string userId)
    {
        var list = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Dish)
            .ThenInclude(d => d.Nutrition)
            .Include(c => c.Dish)
            .ThenInclude(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(c => c.Dish)
            .ThenInclude(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .OrderBy(c => c.TargetDate)
            .ToListAsync();

        foreach (var item in list)
        {
            item.IsAvailableForDate = true;
        }
        
        return list;
    }

    public async Task AddToCartAsync(CartItems item)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c =>
                c.UserId == item.UserId &&
                c.DishId == item.DishId &&
                c.TargetDate.Date == item.TargetDate.Date);

        if (existing != null)
        {
            existing.Quantity += Math.Max(item.Quantity, 1);
        }
        else
        {
            item.Quantity = Math.Max(item.Quantity, 1);
            _context.CartItems.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(string userId, int cartItemId, int quantity, string? note)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cartItemId);

        if (item != null)
        {
            item.Quantity = quantity;

            if (note != null)
                item.Note = note;

            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cartItemId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids)
    {
        return await _context.CartItems
            .Include(c => c.Dish)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
    }

    public async Task RemoveItemsByIdsAsync(List<int> ids)
    {
        var items = await _context.CartItems
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
    
    public async Task MarkAvailabilityForDateAsync(List<CartItems> items, DateTime selectedDate)
    {
        foreach (var item in items)
        {
            item.IsAvailableForDate = await _context.MonthlyMenu.AnyAsync(m =>
                m.DishId == item.DishId &&
                m.Month == selectedDate.Month &&
                m.Year == selectedDate.Year &&
                m.DayOfWeek == selectedDate.DayOfWeek);
        }
    }
}
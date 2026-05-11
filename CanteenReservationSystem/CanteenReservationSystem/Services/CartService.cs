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
        return await _context.CartItems
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
    }

    public async Task<CartItems?> GetItemAsync(string userId, int dishId, DateTime targetDate)
    {
        return await _context.CartItems
            .Include(c => c.Dish)
            .FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.DishId == dishId &&
                c.TargetDate.Date == targetDate.Date);
    }

    public async Task AddToCartAsync(CartItems item)
    {
        var existing = await GetItemAsync(item.UserId, item.DishId, item.TargetDate);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            _context.CartItems.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(string userId, int dishId, DateTime targetDate, int quantity)
    {
        var item = await GetItemAsync(userId, dishId, targetDate);

        if (item != null)
        {
            item.Quantity = quantity;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemAsync(string userId, int dishId, DateTime targetDate)
    {
        var item = await GetItemAsync(userId, dishId, targetDate);

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
}
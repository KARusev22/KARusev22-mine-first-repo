using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class NutritionService : INutritionService
{
    private readonly ApplicationDbContext _context;

    public NutritionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Nutrition?> GetByDishIdAsync(int dishId)
    {
        return await _context.Nutritions
            .Include(n => n.Dish)
            .FirstOrDefaultAsync(n => n.DishId == dishId);
    }

    public async Task CreateAsync(Nutrition nutrition)
    {
        _context.Nutritions.Add(nutrition);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Nutrition nutrition)
    {
        _context.Nutritions.Update(nutrition);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByDishIdAsync(int dishId)
    {
        var nutrition = await _context.Nutritions
            .FirstOrDefaultAsync(n => n.DishId == dishId);

        if (nutrition != null)
        {
            _context.Nutritions.Remove(nutrition);
            await _context.SaveChangesAsync();
        }
    }
}
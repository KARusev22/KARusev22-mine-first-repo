using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class DishService : IDishService
{
    private readonly ApplicationDbContext _context;

    public DishService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Dish>> GetAllAsync()
    {
        return await _context.Dishes
            .Include(d => d.Category)
            .Include(d => d.Nutrition)
            .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .ToListAsync();
    }

    public async Task<Dish?> GetByIdAsync(int id)
    {
        return await _context.Dishes
            .Include(d => d.Category)
            .Include(d => d.Nutrition)
            .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .Include(d => d.MonthlyMenuEntries) 
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task CreateAsync(Dish dish)
    {
        _context.Dishes.Add(dish);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Dish dish)
    {
        _context.Dishes.Update(dish);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var dish = await _context.Dishes.FindAsync(id);
        if (dish == null) return;

        dish.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    //Restores a previously soft-deleted dish
    //IgnoreQueryFilters() is required to access deleted records
    public async Task RestoreAsync(int id)
    {
        var dish = await _context.Dishes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dish == null) return;

        dish.IsDeleted = false;
        await _context.SaveChangesAsync();
    }
    
    //Returns all soft-deleted dishes
    public async Task<IEnumerable<Dish>> GetDeletedAsync()
    {
        return await _context.Dishes
            .IgnoreQueryFilters()
            .Where(x => x.IsDeleted)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Dish>> FilterByCategoryAsync(int categoryId)
    {
        return await _context.Dishes
            .Where(d => d.CategoryId == categoryId)
            .Include(d => d.Nutrition)
            .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .ToListAsync();
    }
}
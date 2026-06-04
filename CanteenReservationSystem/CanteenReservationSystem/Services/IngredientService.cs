using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class IngredientService : IIngredientService
{
    private readonly ApplicationDbContext _context;

    public IngredientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ingredient>> GetAllAsync()
    {
        return await _context.Ingredients
            .Include(i => i.DishIngredients)
            .ThenInclude(di => di.Dish)
            .ToListAsync();
    }

    public async Task<Ingredient?> GetByIdAsync(int id)
    {
        return await _context.Ingredients
            .Include(i => i.DishIngredients)
            .ThenInclude(di => di.Dish)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CreateAsync(Ingredient ingredient)
    {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ingredient ingredient)
    {
        _context.Ingredients.Update(ingredient);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient != null)
        {
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Dish>> GetDishesByIngredientAsync(int ingredientId)
    {
        return await _context.DishIngredients
            .Where(di => di.IngredientId == ingredientId)
            .Include(di => di.Dish)
            .ThenInclude(d => d.Nutrition)
            .Include(di => di.Dish)
            .ThenInclude(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .Select(di => di.Dish)
            .ToListAsync();
    }
    
    public async Task<Ingredient> FindOrCreateByNameAsync(string name)
    {
        var normalized = name.Trim().ToLower();

        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.IngredientName.ToLower() == normalized);

        if (ingredient != null)
            return ingredient;

        ingredient = new Ingredient
        {
            IngredientName = name.Trim()
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        return ingredient;
    }
}
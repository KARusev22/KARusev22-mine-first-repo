using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class AllergenService : IAllergenService
{
    private readonly ApplicationDbContext _context;

    public AllergenService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Allergen>> GetAllAsync()
    {
        return await _context.Allergens
            .Include(a => a.DishAllergens)
            .ThenInclude(da => da.Dish)
            .ToListAsync();
    }

    //Returns a single allergen with its related dishes
    public async Task<Allergen?> GetByIdAsync(int id)
    {
        return await _context.Allergens
            .Include(a => a.DishAllergens)
            .ThenInclude(da => da.Dish)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task CreateAsync(Allergen allergen)
    {
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Allergen allergen)
    {
        _context.Allergens.Update(allergen);
        await _context.SaveChangesAsync();
    }

    // Deletes an allergen if it exists.
    // EF Core will remove related join table entries depending on cascade rules
    public async Task DeleteAsync(int id)
    {
        var allergen = await _context.Allergens.FindAsync(id);
        if (allergen != null)
        {
            _context.Allergens.Remove(allergen);
            await _context.SaveChangesAsync();
        }
    }

    //Returns all dishes associated with a specific allergen
    public async Task<IEnumerable<Dish>> GetDishesByAllergenAsync(int allergenId)
    {
        return await _context.DishAllergens
            .Where(da => da.AllergenId == allergenId)
            .Include(da => da.Dish)
            .ThenInclude(d => d.Nutrition)
            .Include(da => da.Dish)
            .ThenInclude(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(da => da.Dish)
            .ThenInclude(d => d.DishAllergens)
            .ThenInclude(a => a.Allergen)
            .Select(da => da.Dish)
            .ToListAsync();
    }
}
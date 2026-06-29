using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    //Returns all categories including their related dishes
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Dishes)
            .ToListAsync();
    }

    //Returns a single category with full dish details
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Dishes)
            .ThenInclude(d => d.Nutrition)
            .Include(c => c.Dishes)
            .ThenInclude(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(c => c.Dishes)
            .ThenInclude(d => d.DishAllergens)
            .ThenInclude(da => da.Allergen)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    //Creates a new category
    public async Task CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    //Deletes a category if it exists.
    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    //Returns all dishes belonging to a specific category
    public async Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId)
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
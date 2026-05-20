using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IIngredientService
{
    Task<IEnumerable<Ingredient>> GetAllAsync();
    Task<Ingredient?> GetByIdAsync(int id);
    Task CreateAsync(Ingredient ingredient);
    Task UpdateAsync(Ingredient ingredient);
    Task DeleteAsync(int id);
    Task<IEnumerable<Dish>> GetDishesByIngredientAsync(int ingredientId);
    Task<Ingredient> FindOrCreateByNameAsync(string name);
}
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId);
    Task CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}
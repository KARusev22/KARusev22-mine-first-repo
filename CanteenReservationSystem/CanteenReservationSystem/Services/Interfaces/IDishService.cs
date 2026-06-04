using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IDishService
{
    Task<IEnumerable<Dish>> GetAllAsync();
    Task<Dish?> GetByIdAsync(int id);
    Task CreateAsync(Dish dish);
    Task UpdateAsync(Dish dish);
    Task DeleteAsync(int id);
    
    Task<IEnumerable<Dish>> FilterByCategoryAsync(int categoryId);
}
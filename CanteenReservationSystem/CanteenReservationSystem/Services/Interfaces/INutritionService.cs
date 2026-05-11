using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface INutritionService
{
    Task<Nutrition?> GetByDishIdAsync(int dishId);
    Task CreateAsync(Nutrition nutrition);
    Task UpdateAsync(Nutrition nutrition);
    Task DeleteByDishIdAsync(int dishId);
}
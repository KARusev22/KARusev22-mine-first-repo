using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IAllergenService
{
    Task<IEnumerable<Allergen>> GetAllAsync();
    Task<Allergen?> GetByIdAsync(int id);
    Task CreateAsync(Allergen allergen);
    Task UpdateAsync(Allergen allergen);
    Task DeleteAsync(int id);
}
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItems>> GetUserCartAsync(string userId);
    Task<CartItems?> GetItemAsync(string  userId, int dishId, DateTime targetDate);
    Task AddToCartAsync(CartItems item);
    Task UpdateQuantityAsync(string userId, int dishId, DateTime targetDate, int quantity);
    Task RemoveItemAsync(string userId, int dishId, DateTime targetDate);
    Task ClearCartAsync(string userId);
    Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids);
    Task RemoveItemsByIdsAsync(List<int> ids);
}
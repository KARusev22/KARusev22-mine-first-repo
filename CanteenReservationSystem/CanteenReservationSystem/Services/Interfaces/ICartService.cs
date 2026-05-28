using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItems>> GetUserCartAsync(string userId);
    Task AddToCartAsync(CartItems item);
    Task UpdateItemAsync(string userId, int cartItemId, int quantity, string? note);
    Task RemoveItemAsync(string userId, int cartItemId);
    Task ClearCartAsync(string userId);
    Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids);
    Task RemoveItemsByIdsAsync(List<int> ids);
}
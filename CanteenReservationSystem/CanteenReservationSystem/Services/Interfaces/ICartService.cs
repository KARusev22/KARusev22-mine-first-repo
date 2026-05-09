using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItems>> GetUserCartAsync(string userId);
    Task AddToCartAsync(CartItems item);
    Task UpdateCartItemAsync(CartItems item);
    Task RemoveFromCartAsync(int id);
}
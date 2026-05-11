using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<Orders>> GetUserOrdersAsync(string userId);
    Task<Orders?> GetByIdAsync(int id);
    Task<Orders> CreateOrderAsync(string userId, DateTime targetDate);
    Task CancelOrderAsync(int orderId);
}
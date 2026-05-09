using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IOrderService
{
    Task<Orders?> CreateOrderAsync(string userId);
    Task<IEnumerable<Orders>> GetUserOrdersAsync(string userId);
    Task<Orders?> GetByIdAsync(int id);
}
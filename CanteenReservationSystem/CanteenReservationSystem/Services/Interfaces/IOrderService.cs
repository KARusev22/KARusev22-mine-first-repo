using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IOrderService
{
    Task<List<Orders>> GetOrdersByUserAsync(string userId);
    Task<UserStatsViewModel> GetUserStatsAsync(string userId);
}
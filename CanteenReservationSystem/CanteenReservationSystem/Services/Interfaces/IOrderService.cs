using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IOrderService
{
    Task<List<Orders>> GetOrdersByUserAsync(string userId);
    Task<UserStatsViewModel> GetUserStatsAsync(string userId);
    
    Task<Orders> GetByIdForUserAsync(int id, string userId);
    Task DeleteAsync(Orders order);
    Task<string?> UpdateAsync(Orders order, EditOrderViewModel model);
    Task<Orders?> GetByUniqueCodeAsync(string code);

    Task MarkAsCompletedAsync(int orderId);

    Task MarkAllPendingAsNotTakenAsync();
    
    Task<Orders?> GetByIdAsync(int id);
}



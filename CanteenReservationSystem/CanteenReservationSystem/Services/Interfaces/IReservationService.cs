using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IReservationService
{
    Task<Orders> CreateReservationAsync(string userId, DateTime targetDate, List<int> selectedItemIds);
    Task<Orders?> GetByIdAsync(int id);
    Task<Orders?> GetByCodeAsync(string code);
    Task UpdateStatusAsync(int id, string status);
}


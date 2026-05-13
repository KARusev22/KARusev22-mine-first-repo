using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<Payments>> GetPaymentsForOrderAsync(int orderId);
    Task<Payments?> GetByIdAsync(int id);

    Task<Payments> CreatePaymentAsync(int orderId, decimal amount);
}
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payments>> GetPaymentsForOrderAsync(int orderId)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId)
            .Include(p => p.Order)
            .ToListAsync();
    }

    public async Task<Payments?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payments> CreatePaymentAsync(int orderId, decimal amount)
    {
        var payment = new Payments
        {
            OrderId = orderId,
            Amount = amount,
            PaymentDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }
}
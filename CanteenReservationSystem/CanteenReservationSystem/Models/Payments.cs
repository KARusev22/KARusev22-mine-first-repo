namespace CanteenReservationSystem.Models;

public class Payments : BaseEntity
{
    public int OrderId { get; set; }
    public Orders Order { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}
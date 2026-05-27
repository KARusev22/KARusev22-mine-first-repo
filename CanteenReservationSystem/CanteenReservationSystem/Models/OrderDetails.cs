namespace CanteenReservationSystem.Models;

public class OrderDetails : BaseEntity
{
    public int OrderId { get; set; }
    public Orders Order { get; set; }

    public int DishId { get; set; }
    public Dish Dish { get; set; }

    public int Quantity { get; set; }
    public string? Note { get; set; }
}
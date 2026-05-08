namespace CanteenReservationSystem.Models;

public class CartItems : BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int DishId { get; set; }
    public Dish Dish { get; set; }

    public DateTime TargetDate { get; set; }
    public int Quantity { get; set; }
    public string Note { get; set; }
}
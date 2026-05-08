namespace CanteenReservationSystem.Models;

public class Category : BaseEntity
{
    public string CategoryName { get; set; }
    
    public ICollection<Dish> Dishes { get; set; }
}
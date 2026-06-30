namespace CanteenReservationSystem.Models;

public class MonthlyMenu
{
    public int Id { get; set; }

    public int? DishId { get; set; }
    public Dish? Dish { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public int Month { get; set; }
    public int Year { get; set; } 
}
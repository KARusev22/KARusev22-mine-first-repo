namespace CanteenReservationSystem.Models;

public class Nutrition : BaseEntity
{
    public int DishId { get; set; }
    public Dish Dish { get; set; }

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Fats { get; set; }
    public int Carbohydrates { get; set; }
    public int Fiber { get; set; }
    public int WeightGrams { get; set; }
}
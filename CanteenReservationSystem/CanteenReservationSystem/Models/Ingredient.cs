namespace CanteenReservationSystem.Models;

public class Ingredient : BaseEntity
{
    public string IngredientName { get; set; }
    
    public ICollection<DishIngredient> DishIngredients { get; set; }
}
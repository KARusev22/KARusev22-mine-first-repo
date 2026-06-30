namespace CanteenReservationSystem.Models;

public class Ingredient : BaseEntity
{
    public string IngredientName { get; set; }

    // Current stock on hand (grams). Used by the kitchen dashboard and the AI
    // availability assistant to decide whether a day's orders can be prepared.
    public int AvailableGrams { get; set; }

    public ICollection<DishIngredient> DishIngredients { get; set; }
}
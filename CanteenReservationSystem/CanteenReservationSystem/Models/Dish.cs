namespace CanteenReservationSystem.Models;

public class Dish : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public string DishName { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public string Characteristics { get; set; }
    public string Description { get; set; }
    public Nutrition Nutrition { get; set; }
    
    public ICollection<DishIngredient> DishIngredients { get; set; }
    public ICollection<DishAllergen> DishAllergens { get; set; }
}
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace CanteenReservationSystem.Models;

public class Dish : BaseEntity
{
    public int CategoryId { get; set; }
    [ValidateNever] public Category? Category { get; set; }

    public string DishName { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Characteristics { get; set; }
    public string? Description { get; set; }
    [ValidateNever] public Nutrition? Nutrition { get; set; }
    
    [ValidateNever] public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
    [ValidateNever] public ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();
    [ValidateNever] public ICollection<MonthlyMenu> MonthlyMenuEntries { get; set; } = new List<MonthlyMenu>();
}
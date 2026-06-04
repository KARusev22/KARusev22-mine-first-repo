using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CanteenReservationSystem.Models.ViewModels;

using CanteenReservationSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class DishFormViewModel
{
    public Dish Dish { get; set; } = new Dish();
    [ValidateNever]
    public Nutrition Nutrition { get; set; } = new Nutrition();
    [ValidateNever]
    public List<SelectListItem> Categories { get; set; } = new();
    [ValidateNever]
    public List<Allergen> Allergens { get; set; } = new();
    [ValidateNever]
    public List<int> IngredientIds { get; set; } = new();
    [ValidateNever]
    public List<string> IngredientNames { get; set; } = new();
    [ValidateNever]
    public List<int> IngredientGrams { get; set; } = new();
    [ValidateNever]
    public List<int> SelectedAllergenIds { get; set; } = new();
}
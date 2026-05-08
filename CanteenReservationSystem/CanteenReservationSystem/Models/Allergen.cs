namespace CanteenReservationSystem.Models;

public class Allergen : BaseEntity
{
    public string AllergenName { get; set; }
    public string IconUrl { get; set; }
    
    public ICollection<DishAllergen> DishAllergens { get; set; }
}
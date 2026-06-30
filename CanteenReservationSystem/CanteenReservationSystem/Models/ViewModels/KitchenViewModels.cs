namespace CanteenReservationSystem.Models.ViewModels;

public class KitchenViewModel
{
    public DateTime SelectedDate { get; set; }

    public List<KitchenDishViewModel> Dishes { get; set; } = new();
    public List<KitchenCategoryViewModel> Categories { get; set; } = new();
    public List<KitchenIngredientViewModel> Ingredients { get; set; } = new();

    // Full ingredient stock list for the inventory editor.
    public List<KitchenStockViewModel> Stock { get; set; } = new();
}

public class KitchenStockViewModel
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public int AvailableGrams { get; set; }
}

public class KitchenDishViewModel
{
    public string DishName { get; set; } = string.Empty;
    public int TotalPortions { get; set; }
    public List<KitchenNoteViewModel> Notes { get; set; } = new();
}

public class KitchenNoteViewModel
{
    public int Quantity { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class KitchenCategoryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalPortions { get; set; }
}

public class KitchenIngredientViewModel
{
    public string IngredientName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public string Unit { get; set; } = "g";

    // Stock on hand for this ingredient and whether it covers the day's needs.
    public int AvailableQuantity { get; set; }
    public bool IsSufficient => AvailableQuantity >= TotalQuantity;
}
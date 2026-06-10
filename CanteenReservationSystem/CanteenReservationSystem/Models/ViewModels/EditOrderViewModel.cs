namespace CanteenReservationSystem.Models.ViewModels;

public class EditOrderViewModel
{
    public int OrderId { get; set; }
    public DateTime TargetDate { get; set; }
    public string? Note { get; set; }

    public List<EditOrderItemViewModel> Items { get; set; }
    public List<DishOption> AllDishes { get; set; }
}

public class EditOrderItemViewModel
{
    public int OrderDetailId { get; set; }
    public int DishId { get; set; }
    public string? DishName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class DishOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
namespace CanteenReservationSystem.Models.ViewModels;

public class OrderViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }

    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
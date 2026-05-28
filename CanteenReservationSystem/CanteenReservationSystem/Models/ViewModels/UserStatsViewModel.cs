namespace CanteenReservationSystem.Models.ViewModels;

public class UserStatsViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string? MostOrderedDish { get; set; }
}
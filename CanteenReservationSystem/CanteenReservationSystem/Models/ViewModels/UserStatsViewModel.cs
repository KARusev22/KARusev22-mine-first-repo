namespace CanteenReservationSystem.Models.ViewModels;

public class UserStatsViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string? MostOrderedDish { get; set; }
    public int TakenOrders { get; set; }
    public int NotTakenOrders { get; set; }
    
    public Dictionary<string, int> Top3Days { get; set; } = new();
    public Dictionary<string, int> Top3Categories { get; set; } = new();
}
namespace CanteenReservationSystem.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalOrders { get; set; }
    public int OrdersToday { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    
    public List<string> OrdersPerDay_Labels { get; set; }
    public List<int> OrdersPerDay_Values { get; set; }

    public List<string> StatusLabels { get; set; }
    public List<int> StatusValues { get; set; }

    public List<string> TopDishes_Labels { get; set; }
    public List<int> TopDishes_Values { get; set; }

    public List<string> RevenuePerDay_Labels { get; set; }
    public List<decimal> RevenuePerDay_Values { get; set; }

    public List<string> BlackPoints_Labels { get; set; }
    public List<int> BlackPoints_Values { get; set; }

    public List<string> ActiveUsers_Labels { get; set; }
    public List<int> ActiveUsers_Values { get; set; }
}
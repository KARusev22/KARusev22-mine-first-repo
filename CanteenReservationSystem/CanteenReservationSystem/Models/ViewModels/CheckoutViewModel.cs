namespace CanteenReservationSystem.Models.ViewModels;

public class CheckoutViewModel
{
    public List<CartItems> Items { get; set; }
    public DateTime SelectedDate { get; set; }
}
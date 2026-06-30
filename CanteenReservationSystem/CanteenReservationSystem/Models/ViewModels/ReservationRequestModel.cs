namespace CanteenReservationSystem.Models.ViewModels;

public class ReservationRequestModel
{
    public List<int> SelectedItemIds { get; set; } = new();
    public DateTime TargetDate { get; set; }
}

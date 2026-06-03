namespace CanteenReservationSystem.Models;

public class Orders : BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public string UniqueCode { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } // Pending, Completed, NotTaken
    public DateTime CreatedAt { get; set; }

    public DateTime TargetDate { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
}
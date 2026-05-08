namespace CanteenReservationSystem.Models;

public class Polls : BaseEntity
{
    public string Question { get; set; }
    public bool IsActive { get; set; }

    public ICollection<PollOptions> Options { get; set; }
}
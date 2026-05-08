namespace CanteenReservationSystem.Models;

public class PollOptions : BaseEntity
{
    public int PollId { get; set; }
    public Polls Poll { get; set; }

    public string OptionText { get; set; }

    public ICollection<Votes> Votes { get; set; }
}
namespace CanteenReservationSystem.Models;

public class Votes : BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public DateTime VotedAt { get; set; }
    public int OptionId { get; set; }
    public PollOptions Option { get; set; } 
    
}
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IPollService
{
    Task<Polls?> GetActivePollAsync();
    Task<IEnumerable<PollOptions>> GetOptionsAsync(int pollId);
    Task VoteAsync(Votes vote);
}
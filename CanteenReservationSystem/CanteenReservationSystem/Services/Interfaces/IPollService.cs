using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IPollService
{
    Task<IEnumerable<Polls>> GetActivePollsAsync();
    Task<Polls?> GetByIdAsync(int id);

    Task CreatePollAsync(Polls poll, IEnumerable<string> options);
    Task VoteAsync(int pollId, int optionId, string userId);

    Task<bool> HasUserVotedAsync(int pollId, string userId);
    Task<IEnumerable<PollOptions>> GetOptionsAsync(int pollId);
}
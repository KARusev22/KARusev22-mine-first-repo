using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IVoteService
{
    Task<bool> HasUserVotedAsync(int pollId, string userId);
    Task<Votes> AddVoteAsync(int optionId, string userId);
    Task<IEnumerable<Votes>> GetVotesForPollAsync(int pollId);
}
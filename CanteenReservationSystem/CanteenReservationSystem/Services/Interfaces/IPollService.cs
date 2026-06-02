using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IPollService
{
    Task<IEnumerable<Polls>> GetActivePollsAsync();
    Task<Polls?> GetByIdAsync(int id);
    Task CreatePollAsync(Polls poll, IEnumerable<string> options);
    Task<bool> HasUserVotedAsync(int pollId, string userId);
    Task<IEnumerable<PollOptions>> GetOptionsAsync(int pollId);
    Task DeletePollAsync(int pollId);
    Task<IEnumerable<Polls>> GetAllPollsAsync();
    Task<Dictionary<string, int>> GetPollResultsAsync(int pollId);
}
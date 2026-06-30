using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class VoteService : IVoteService
{
    private readonly ApplicationDbContext _context;

    public VoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasUserVotedAsync(int pollId, string userId)
    {
        return await _context.Votes
            .Include(v => v.Option)
            .AnyAsync(v => v.Option.PollId == pollId && v.UserId == userId);
    }

    //Stores timestamp for auditing or analytics
    public async Task<Votes> AddVoteAsync(int optionId, string userId)
    {
        var vote = new Votes
        {
            OptionId = optionId,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        return vote;
    }

    //Retrieves all votes for a specific poll
    public async Task<IEnumerable<Votes>> GetVotesForPollAsync(int pollId)
    {
        return await _context.Votes
            .Include(v => v.Option)
            .Where(v => v.Option.PollId == pollId)
            .ToListAsync();
    }
}
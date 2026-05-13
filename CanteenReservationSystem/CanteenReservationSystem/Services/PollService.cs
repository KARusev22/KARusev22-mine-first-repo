using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class PollService : IPollService
{
    private readonly ApplicationDbContext _context;

    public PollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Polls>> GetActivePollsAsync()
    {
        return await _context.Polls
            .Where(p => p.IsActive)
            .Include(p => p.Options)
            .ToListAsync();
    }

    public async Task<Polls?> GetByIdAsync(int id)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreatePollAsync(Polls poll, IEnumerable<string> options)
    {
        poll.IsActive = true;
        poll.CreatedAt = DateTime.UtcNow;

        poll.Options = options
            .Select(o => new PollOptions
            {
                OptionText = o
            })
            .ToList();

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
    }

    public async Task VoteAsync(int pollId, int optionId, string userId)
    {
        if (await HasUserVotedAsync(pollId, userId))
            throw new InvalidOperationException("User has already voted.");

        var vote = new Votes
        {
            OptionId = optionId,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserVotedAsync(int pollId, string userId)
    {
        return await _context.Votes
            .Include(v => v.Option)
            .AnyAsync(v => v.Option.PollId == pollId && v.UserId == userId);
    }

    public async Task<IEnumerable<PollOptions>> GetOptionsAsync(int pollId)
    {
        return await _context.PollOptions
            .Where(o => o.PollId == pollId)
            .Include(o => o.Votes)
            .ToListAsync();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class PollServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetActivePollsAsync_ReturnsOnlyActive()
    {
        using var context = CreateContext("GetActivePollsAsync_ReturnsOnlyActive");
        context.Polls.Add(new Polls { Question = "Q1", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Polls.Add(new Polls { Question = "Q2", IsActive = false, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new PollService(context);
        var results = (await service.GetActivePollsAsync()).ToList();

        Assert.Single(results);
        Assert.Equal("Q1", results[0].Question);
    }

    [Fact]
    public async Task CreatePollAsync_AddsPollAndOptions()
    {
        using var context = CreateContext("CreatePollAsync_AddsPollAndOptions");
        var service = new PollService(context);

        var poll = new Polls { Question = "Best dish?" };
        await service.CreatePollAsync(poll, new[] { "A", "B", "C" });

        var stored = await context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Question == "Best dish?");
        Assert.NotNull(stored);
        Assert.True(stored!.IsActive);
        Assert.Equal(3, stored.Options.Count);
    }

    [Fact]
    public async Task HasUserVotedAsync_ReturnsTrueWhenVoted()
    {
        using var context = CreateContext("HasUserVotedAsync_ReturnsTrueWhenVoted");
        var poll = new Polls { Question = "Q", IsActive = true, CreatedAt = DateTime.UtcNow };
        var option = new PollOptions { OptionText = "Opt", Poll = poll };
        var vote = new Votes { UserId = "u1", Option = option };

        context.Polls.Add(poll);
        context.PollOptions.Add(option);
        context.Votes.Add(vote);
        await context.SaveChangesAsync();

        var service = new PollService(context);
        var voted = await service.HasUserVotedAsync(poll.Id, "u1");

        Assert.True(voted);
    }

    [Fact]
    public async Task GetOptionsAsync_ReturnsOptionsWithVotes()
    {
        using var context = CreateContext("GetOptionsAsync_ReturnsOptionsWithVotes");
        var poll = new Polls { Question = "Colors", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Polls.Add(poll);
        await context.SaveChangesAsync();

        var o1 = new PollOptions { OptionText = "Red", PollId = poll.Id };
        var o2 = new PollOptions { OptionText = "Blue", PollId = poll.Id };
        context.PollOptions.AddRange(o1, o2);
        await context.SaveChangesAsync();

        var service = new PollService(context);
        var options = (await service.GetOptionsAsync(poll.Id)).ToList();

        Assert.Equal(2, options.Count);
        Assert.All(options, o => Assert.Equal(poll.Id, o.PollId));
    }

    [Fact]
    public async Task DeletePollAsync_RemovesPollAndCascade()
    {
        using var context = CreateContext("DeletePollAsync_RemovesPollAndCascade");
        var poll = new Polls { Question = "Temp", IsActive = true, CreatedAt = DateTime.UtcNow };
        var option = new PollOptions { OptionText = "o", Poll = poll };
        var vote = new Votes { UserId = "u2", Option = option };

        context.Polls.Add(poll);
        context.PollOptions.Add(option);
        context.Votes.Add(vote);
        await context.SaveChangesAsync();

        var service = new PollService(context);
        await service.DeletePollAsync(poll.Id);

        var storedPoll = await context.Polls.FindAsync(poll.Id);
        var storedOption = await context.PollOptions.FirstOrDefaultAsync(o => o.PollId == poll.Id);
        var storedVote = await context.Votes.FirstOrDefaultAsync(v => v.Option.PollId == poll.Id);

        Assert.Null(storedPoll);
        Assert.Null(storedOption);
        Assert.Null(storedVote);
    }
}

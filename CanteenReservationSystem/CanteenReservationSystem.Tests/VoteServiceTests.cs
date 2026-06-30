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

public class VoteServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HasUserVotedAsync_ReturnsFalseIfNot()
    {
        using var context = CreateContext("HasUserVotedAsync_ReturnsFalseIfNot");
        var poll = new Polls { Question = "P", IsActive = true, CreatedAt = DateTime.UtcNow };
        var option = new PollOptions { OptionText = "o", Poll = poll };
        context.Polls.Add(poll);
        context.PollOptions.Add(option);
        await context.SaveChangesAsync();

        var service = new VoteService(context);
        var res = await service.HasUserVotedAsync(poll.Id, "uX");
        Assert.False(res);
    }

    [Fact]
    public async Task AddVoteAsync_AddsVote()
    {
        using var context = CreateContext("AddVoteAsync_AddsVote");
        var poll = new Polls { Question = "P2", IsActive = true, CreatedAt = DateTime.UtcNow };
        var option = new PollOptions { OptionText = "o2", Poll = poll };
        context.Polls.Add(poll);
        context.PollOptions.Add(option);
        await context.SaveChangesAsync();

        var service = new VoteService(context);
        var vote = await service.AddVoteAsync(option.Id, "user10");

        Assert.NotNull(vote);
        Assert.Equal(option.Id, vote.OptionId);
        Assert.Equal("user10", vote.UserId);

        var stored = await context.Votes.FindAsync(vote.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task GetVotesForPollAsync_ReturnsVotes()
    {
        using var context = CreateContext("GetVotesForPollAsync_ReturnsVotes");
        var poll = new Polls { Question = "P3", IsActive = true, CreatedAt = DateTime.UtcNow };
        var option = new PollOptions { OptionText = "o3", Poll = poll };
        var vote1 = new Votes { UserId = "a", Option = option };
        var vote2 = new Votes { UserId = "b", Option = option };

        context.Polls.Add(poll);
        context.PollOptions.Add(option);
        context.Votes.AddRange(vote1, vote2);
        await context.SaveChangesAsync();

        var service = new VoteService(context);
        var votes = (await service.GetVotesForPollAsync(poll.Id)).ToList();

        Assert.Equal(2, votes.Count);
        Assert.All(votes, v => Assert.Equal(poll.Id, v.Option.PollId));
    }
}

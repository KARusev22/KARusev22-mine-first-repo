using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class PollControllerTests
{
    private ControllerContext CreateControllerContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private TempDataDictionary CreateTempData()
    {
        var httpContext = new DefaultHttpContext();
        return new TempDataDictionary(httpContext, new FakeTempDataProvider());
    }

    private class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private class FakePollService : IPollService
    {
        public bool CreatePollCalled { get; private set; }
        public bool DeletePollCalled { get; private set; }
        public int DeletedPollId { get; private set; }
        public Func<Task<IEnumerable<Polls>>> GetActivePollsImpl { get; set; } = () => Task.FromResult<IEnumerable<Polls>>(new List<Polls>());
        public Func<int, Task<Polls?>> GetByIdImpl { get; set; } = id => Task.FromResult<Polls?>(null);
        public Func<Polls, IEnumerable<string>, Task>? CreatePollAsyncImpl { get; set; }
        public Func<int, string, Task<bool>> HasUserVotedAsyncImpl { get; set; } = (_, _) => Task.FromResult(false);
        public Func<int, Task<IEnumerable<PollOptions>>> GetOptionsAsyncImpl { get; set; } = _ => Task.FromResult<IEnumerable<PollOptions>>(new List<PollOptions>());

        public Task<IEnumerable<Polls>> GetActivePollsAsync() => GetActivePollsImpl();
        public Task<Polls?> GetByIdAsync(int id) => GetByIdImpl(id);
        public Task CreatePollAsync(Polls poll, IEnumerable<string> options)
        {
            CreatePollCalled = true;
            return CreatePollAsyncImpl?.Invoke(poll, options) ?? Task.CompletedTask;
        }
        public Task<bool> HasUserVotedAsync(int pollId, string userId) => HasUserVotedAsyncImpl(pollId, userId);
        public Task<IEnumerable<PollOptions>> GetOptionsAsync(int pollId) => GetOptionsAsyncImpl(pollId);
        public Task DeletePollAsync(int pollId)
        {
            DeletePollCalled = true;
            DeletedPollId = pollId;
            return Task.CompletedTask;
        }
    }

    private class FakeVoteService : IVoteService
    {
        public Func<int, string, Task<bool>> HasUserVotedAsyncImpl { get; set; } = (_, _) => Task.FromResult(false);
        public Func<int, string, Task<Votes>> AddVoteAsyncImpl { get; set; } = (optionId, userId) => Task.FromResult(new Votes { OptionId = optionId, UserId = userId });

        public Task<bool> HasUserVotedAsync(int pollId, string userId) => HasUserVotedAsyncImpl(pollId, userId);
        public Task<Votes> AddVoteAsync(int optionId, string userId) => AddVoteAsyncImpl(optionId, userId);
        public Task<IEnumerable<Votes>> GetVotesForPollAsync(int pollId) => Task.FromResult<IEnumerable<Votes>>(new List<Votes>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithPolls()
    {
        var expectedPolls = new List<Polls> { new Polls { Id = 1, Question = "Q1" } };
        var pollService = new FakePollService { GetActivePollsImpl = () => Task.FromResult<IEnumerable<Polls>>(expectedPolls) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedPolls, view.Model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenPollDoesNotExist()
    {
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(null) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Details(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithPoll_WhenPollExists()
    {
        var poll = new Polls { Id = 2, Question = "Do you like tests?" };
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(poll) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Details(2);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(poll, view.Model);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var controller = new PollController(new FakePollService(), new FakeVoteService());

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_ReturnsView_WhenNoOptions()
    {
        var poll = new Polls { Question = "New poll" };
        var pollService = new FakePollService();
        var controller = new PollController(pollService, new FakeVoteService())
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Create(poll, new List<string> { "" });

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(poll, view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task Create_Post_RedirectsToIndex_WhenOptionsValid()
    {
        var poll = new Polls { Question = "New poll" };
        var pollService = new FakePollService();
        var controller = new PollController(pollService, new FakeVoteService())
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Create(poll, new List<string> { "Option 1", "Option 2" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PollController.Index), redirect.ActionName);
        Assert.True(pollService.CreatePollCalled);
    }

    [Fact]
    public async Task Vote_Post_RedirectsToDetails_WhenUserHasAlreadyVoted()
    {
        var pollService = new FakePollService();
        var voteService = new FakeVoteService { HasUserVotedAsyncImpl = (pollId, userId) => Task.FromResult(true) };
        var controller = new PollController(pollService, voteService)
        {
            ControllerContext = CreateControllerContext("user1"),
            TempData = CreateTempData()
        };

        var result = await controller.Vote(5, 10);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PollController.Details), redirect.ActionName);
        Assert.Equal(5, redirect.RouteValues["id"]);
        Assert.Equal("You have already voted for this poll!", controller.TempData["Error"]);
    }

    [Fact]
    public async Task Vote_Post_RedirectsToResults_WhenUserHasNotVoted()
    {
        var pollService = new FakePollService();
        var voteService = new FakeVoteService { HasUserVotedAsyncImpl = (pollId, userId) => Task.FromResult(false) };
        var controller = new PollController(pollService, voteService)
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Vote(6, 12);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PollController.Results), redirect.ActionName);
        Assert.Equal(6, redirect.RouteValues["id"]);
    }

    [Fact]
    public async Task Results_ReturnsNotFound_WhenPollDoesNotExist()
    {
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(null) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Results(7);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Results_ReturnsViewWithPoll_WhenPollExists()
    {
        var poll = new Polls { Id = 8, Question = "Which is better?" };
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(poll) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Results(8);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(poll, view.Model);
    }

    [Fact]
    public async Task Delete_Post_ReturnsNotFound_WhenPollDoesNotExist()
    {
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(null) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Delete(9);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Post_RedirectsToIndex_WhenPollExists()
    {
        var poll = new Polls { Id = 10, Question = "Delete me" };
        var pollService = new FakePollService { GetByIdImpl = id => Task.FromResult<Polls?>(poll) };
        var controller = new PollController(pollService, new FakeVoteService());

        var result = await controller.Delete(10);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PollController.Index), redirect.ActionName);
        Assert.True(pollService.DeletePollCalled);
        Assert.Equal(10, pollService.DeletedPollId);
    }
}

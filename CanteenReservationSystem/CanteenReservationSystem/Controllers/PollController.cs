using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models;
using System.Security.Claims;

namespace CanteenReservationSystem.Controllers;

public class PollController : Controller
{
    private readonly IPollService _pollService;
    private readonly IVoteService _voteService;

    public PollController(IPollService pollService, IVoteService voteService)
    {
        _pollService = pollService;
        _voteService = voteService;
    }
    public async Task<IActionResult> Index()
    {
        var polls = await _pollService.GetActivePollsAsync();
        return View(polls);
    }
    public async Task<IActionResult> Details(int id)
    {
        var poll = await _pollService.GetByIdAsync(id);
        if (poll == null)
            return NotFound();

        return View(poll);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vote(int pollId, int optionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (await _voteService.HasUserVotedAsync(pollId, userId))
        {
            TempData["Error"] = "You have already voted for this poll!";
            return RedirectToAction(nameof(Details), new { id = pollId });
        }

        await _voteService.AddVoteAsync(optionId, userId);

        return RedirectToAction(nameof(Results), new { id = pollId });
    }

    public async Task<IActionResult> Results(int id)
    {
        var poll = await _pollService.GetByIdAsync(id);
        if (poll == null)
            return NotFound();

        return View(poll);
    }
}
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

    [HttpPost]
    public async Task<IActionResult> SubmitVote(int optionId)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var option = await _pollService.GetOptionByIdAsync(optionId);
        if (option == null)
            return NotFound();

        var pollId = option.PollId;

        if (await _voteService.HasUserVotedAsync(pollId, userId))
            return BadRequest("Already voted");

        await _voteService.AddVoteAsync(optionId, userId);

        return Ok();
    }
}
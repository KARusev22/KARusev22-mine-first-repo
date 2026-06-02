using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminPollController : Controller
{
    private readonly IPollService _pollService;

    public AdminPollController(IPollService pollService)
    {
        _pollService = pollService;
    }

    public async Task<IActionResult> Index()
    {
        var polls = await _pollService.GetAllPollsAsync();
        return View(polls);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Polls poll, List<string> options)
    {
        options = options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

        if (!options.Any())
        {
            ModelState.AddModelError("", "Add at least one option");
            return View(poll);
        }

        await _pollService.CreatePollAsync(poll, options);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Results(int id)
    {
        var poll = await _pollService.GetByIdAsync(id);
        return View(poll);
    }

    public async Task<IActionResult> Delete(int id)
    {
        await _pollService.DeletePollAsync(id);
        return RedirectToAction("Index");
    }
}
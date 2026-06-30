using CanteenReservationSystem.Data;
using CanteenReservationSystem.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Controllers;

// All AI endpoints are invoked asynchronously from the browser (fetch) so they
// never block the initial page render or normal staff workflows.
[Authorize]
public class AiController : Controller
{
    private readonly IAiAssistantService _ai;
    private readonly ApplicationDbContext _context;

    public AiController(IAiAssistantService ai, ApplicationDbContext context)
    {
        _ai = ai;
        _context = context;
    }

    // ---- USER: weekly menu planner page ----
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> WeeklyMenu()
    {
        ViewData["FigustaPage"] = true;
        ViewData["FigustaNav"] = "aiPlanner";
        ViewBag.AiConfigured = _ai.IsConfigured;
        ViewBag.Allergens = await _context.Allergens
            .Select(a => a.AllergenName)
            .ToListAsync();
        return View();
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WeeklyMenu([FromForm] WeeklyMenuRequest request, CancellationToken ct)
    {
        var result = await _ai.BuildWeeklyMenuAsync(request, ct);
        return Json(result);
    }

    // ---- KITCHEN: availability insight for a day ----
    [Authorize(Roles = "Kitchen,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KitchenInsights(DateTime? date, CancellationToken ct)
    {
        var result = await _ai.AnalyzeKitchenDayAsync(date ?? DateTime.Today, ct);
        return Json(result);
    }

    // ---- ADMIN: poll suggestion ----
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PollSuggestion(CancellationToken ct)
    {
        var result = await _ai.SuggestPollAsync(ct);
        return Json(result);
    }

    // ---- CASHIER: order pickup handover summary ----
    [Authorize(Roles = "Cashier,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CashierSummary(string code, CancellationToken ct)
    {
        var result = await _ai.SummarizeOrderForPickupAsync(code, ct);
        return Json(result);
    }
}

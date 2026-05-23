using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models.ViewModels;
using System.Security.Claims;

namespace CanteenReservationSystem.Controllers;

public class ReservationController : Controller
{
    private readonly ICartService _cartService;
    private readonly IReservationService _reservationService;

    public ReservationController(ICartService cartService, IReservationService reservationService)
    {
        _cartService = cartService;
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cart = await _cartService.GetUserCartAsync(userId);

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(ReservationRequestModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (model.SelectedItemIds == null || !model.SelectedItemIds.Any())
        {
            TempData["Error"] = "You must select at least one item";
            return RedirectToAction("Checkout");
        }
        
        var minDate = DateTime.Today.AddDays(1);

        if (model.TargetDate.Date < minDate)
        {
            TempData["Error"] = "You can make a reservation for tomorrow at the earliest.";
            return RedirectToAction("Checkout");
        }

        var reservation = await _reservationService.CreateReservationAsync(
            userId,
            model.TargetDate,
            model.SelectedItemIds
        );

        return RedirectToAction("Success", new { id = reservation.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);

        if (reservation == null)
            return NotFound();

        return View(reservation);
    }

    public IActionResult ManagerLookup()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ManagerLookup(string code)
    {
        var reservation = await _reservationService.GetByCodeAsync(code);

        if (reservation == null)
        {
            TempData["Error"] = "No reservation found with code: " + code;
            return View();
        }

        return View("ManagerDetails", reservation);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _reservationService.UpdateStatusAsync(id, status);
        return RedirectToAction("ManagerLookup");
    }
}
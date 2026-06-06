using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Data;
using System.Security.Claims;

namespace CanteenReservationSystem.Controllers;

public class ReservationController : Controller
{
    private readonly ICartService _cartService;
    private readonly IReservationService _reservationService;
    private readonly ApplicationDbContext _context;
    
    public ReservationController(ICartService cartService, IReservationService reservationService,
        ApplicationDbContext context)
    {
        _cartService = cartService;
        _reservationService = reservationService;
        _context = context;
    }

    public async Task<IActionResult> Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cart = await _cartService.GetUserCartAsync(userId);

        var vm = new CheckoutViewModel
        {
            Items = cart.ToList(),
            SelectedDate = DateTime.Today.AddDays(1)
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(DateTime TargetDate, int[] SelectedItemIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var items = await _cartService.GetItemsByIdsAsync(SelectedItemIds.ToList());

        await _cartService.MarkAvailabilityForDateAsync(items, TargetDate);

        var vm = new CheckoutViewModel
        {
            Items = items,
            SelectedDate = TargetDate
        };

        if (items.Any(i => !i.IsAvailableForDate))
            return View(vm);

        return View("ConfirmReservation", new ConfirmReservationViewModel {
            TargetDate = TargetDate,
            Ids = string.Join(",", SelectedItemIds)
        });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(DateTime TargetDate, string ids)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var selectedIds = ids.Split(',').Select(int.Parse).ToList();

        var reservation = await _reservationService.CreateReservationAsync(
            userId,
            TargetDate,
            selectedIds
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
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CanteenReservationSystem.Controllers;

public class KitchenController : Controller
{
    private readonly IKitchenService _kitchenService;

    public KitchenController(IKitchenService kitchenService)
    {
        _kitchenService = kitchenService;
    }

    public IActionResult Index(DateTime? date)
    {
        ViewData["FigustaNav"] = "kitchenDashboard";
        
        var selectedDate = date ?? DateTime.Today;
        var vm = _kitchenService.GetKitchenData(selectedDate);
        return View(vm);
    }
}
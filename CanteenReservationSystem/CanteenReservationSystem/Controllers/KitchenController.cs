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
        
        //Default to today's date if none is provided
        var selectedDate = date ?? DateTime.Today;
        
        //Retrieve kitchen data
        var vm = _kitchenService.GetKitchenData(selectedDate);
        return View(vm);
    }
}
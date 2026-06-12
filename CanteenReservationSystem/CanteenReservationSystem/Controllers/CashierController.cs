using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models;

namespace CanteenReservationSystem.Controllers;

[Authorize(Roles = "Cashier")]
public class CashierController : Controller
{
    private readonly IOrderService _orderService;

    public CashierController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        ViewData["FigustaPage"] = true;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> FindOrder(string uniqueCode)
    {
        ViewData["FigustaPage"] = true;
        
        var order = await _orderService.GetByUniqueCodeAsync(uniqueCode);

        if (order == null)
        {
            ViewBag.Error = "Invalid code.";
            return View("Index");
        }

        return View("OrderDetails", order);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        ViewData["FigustaPage"] = true;
        
        var order = await _orderService.GetByIdAsync(orderId);

        if (order == null)
        {
            ViewBag.Error = "Order not found.";
            return RedirectToAction("Index");
        }

        if (order.Status == "Completed")
        {
            ViewBag.Error = "This order has already been taken.";
            return View("OrderDetails", order);
        }

        await _orderService.MarkAsCompletedAsync(orderId);

        ViewBag.Success = "Order marked as taken.";
        return View("OrderDetails", order);
    }
}